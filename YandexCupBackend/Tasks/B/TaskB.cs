using System;
using System.Collections.Generic;
using System.Linq;

namespace YandexCupBackend
{
    class Boxes
    {
        public Boxes(List<int> deliveryIds, List<int> parentIds, List<int> missingDeliveryIds)
        {
            DeliveryIds = deliveryIds;
            ParentIds = parentIds;
            MissingDeliveryIds = missingDeliveryIds.ToHashSet();
            PossibleToDeliverIds = DeliveryIds.Select(id => !MissingDeliveryIds.Contains(id)).ToList();
        }
        private List<int> DeliveryIds { get; }
        private List<int> ParentIds { get; }
        private HashSet<int> MissingDeliveryIds { get; }
        private List<bool> PossibleToDeliverIds { get; }
        private List<int> GetPossibleToDeliverPalletIds()
        {
            var palletIds = new List<int>();
            for (int boxId = 0; boxId < ParentIds.Count; boxId++)
            {
                if (IsPossibleToDeliver(boxId) && IsPallet(boxId))
                {
                    palletIds.Add(boxId + 1);
                }
            }
            return palletIds;
        }
        private void CalculatePossibleToDeliverBoxIds()
        {
            for (var boxId = 0; boxId < DeliveryIds.Count; boxId++)
            {
                if (!IsPallet(boxId) && !IsPossibleToDeliver(boxId))
                {
                    SetParentsFalse(boxId);
                }
            }
        }
        public List<int> CalculatePossibleToDeliverPalletIds()
        {
            CalculatePossibleToDeliverBoxIds();
            return GetPossibleToDeliverPalletIds();
        }
        private bool IsPallet(int boxId) => ParentIds[boxId] == -1;
        private bool IsPossibleToDeliver(int boxId) => PossibleToDeliverIds[boxId]; 
        private void SetParentsFalse(int boxId)
        {
            while (!IsPallet(boxId))
            {
                boxId = ParentIds[boxId];
                if (!IsPossibleToDeliver(boxId))
                    return;
                PossibleToDeliverIds[boxId] = false;
            }
            PossibleToDeliverIds[boxId] = false;
        }
    }

    class BoxesReader
    {
        public Boxes ReadBoxes()
        {
            var n = int.Parse(Console.ReadLine());
            var deliveryIds = Console.ReadLine().Split(' ').Select(int.Parse).ToList();
            var parentIds = Console.ReadLine().Split(' ').Select(int.Parse).Select(parentId => parentId - 1).ToList();
            var k = int.Parse(Console.ReadLine());
            var missingDeliveryIds = new List<int>();
            if (k > 0)
            {
                missingDeliveryIds = Console.ReadLine().Split(' ').Select(int.Parse).ToList();
            }
            var boxes = new Boxes(deliveryIds, parentIds, missingDeliveryIds);
            return boxes;
        }
    }

    class TaskB
    {
        public static void Solve()
        {
            var boxes = new BoxesReader().ReadBoxes();
            var palletIds = boxes.CalculatePossibleToDeliverPalletIds();
            Console.WriteLine(palletIds.Count);
            if (palletIds.Count <= 0) return;
            Console.WriteLine(string.Join(Environment.NewLine, palletIds));
        }
    }
}
