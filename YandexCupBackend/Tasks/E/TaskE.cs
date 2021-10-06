using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YandexCupBackend
{
    enum ProductType
    {
        KGT, COLD, OTHER, NULL
    }
    class ProductInfo
    {
        private const string DateFormat = "yyyy-MM-dd";
        public ProductInfo(int warehouseId, ProductType type, DateTime intervalStart, DateTime intervalEnd)
        {
            WarehouseId = warehouseId;
            Type = type;
            IntervalStart = intervalStart;
            IntervalEnd = intervalEnd;
        }

        public int WarehouseId { get; }
        public ProductType Type { get; }
        public DateTime IntervalStart { get; }
        public DateTime IntervalEnd { get; }

        public static ProductInfo FromString(string str)
        {
            //4,2020-02-23 2020-11-01,KGT
            var splitStr = str.Split(new char[] {',', ' '});
            var warehouseId = int.Parse(splitStr[0]);
            Enum.TryParse(splitStr.Last(), true, out ProductType type);
            var start = DateTime.ParseExact(splitStr[1], DateFormat, CultureInfo.InvariantCulture); 
            var end = DateTime.ParseExact(splitStr[2], DateFormat, CultureInfo.InvariantCulture); 
            return new ProductInfo(warehouseId, type, start, end);
        }

        public List<ProductInfo> CorrectInfoNullType()
        {
            var infos = new List<ProductInfo>();
            if (Type != ProductType.NULL)
            {
                infos.Add(this);
            }
            else
            {
                infos.Add(new ProductInfo(WarehouseId, ProductType.COLD, IntervalStart, IntervalEnd));
                infos.Add(new ProductInfo(WarehouseId, ProductType.KGT, IntervalStart, IntervalEnd));
                infos.Add(new ProductInfo(WarehouseId, ProductType.OTHER, IntervalStart, IntervalEnd));
            }

            return infos;
        }

        public static List<ProductInfo> CorrectInfoNullType(IEnumerable<ProductInfo> infos)
        {
            var correctInfos = new List<ProductInfo>();
            foreach (var info in infos)
            {
                if (info.Type == ProductType.NULL)
                {
                    correctInfos.AddRange(info.CorrectInfoNullType());
                }
                else
                {
                    correctInfos.Add(info);
                }
            }

            return correctInfos;
        }

        private bool IsInInterval(DateTime date) => IntervalStart <= date && date <= IntervalEnd;
        private bool IsIntervalIntersect(ProductInfo info)
        {
            return IsInInterval(info.IntervalStart) || IsInInterval(info.IntervalEnd)
                || info.IsInInterval(IntervalStart) || info.IsInInterval(IntervalEnd);

        }

        public bool TryUniteIntervalIntersectsWith(ProductInfo info, out ProductInfo gluedProductInfo)
        {
            gluedProductInfo = null;
            if (WarehouseId != info.WarehouseId || Type != info.Type || info == this)
                return false;
            //if(A1>B2 || B1>A2)
            //if (IntervalStart > IntervalEnd || info.IntervalStart > IntervalEnd)
            //if (IntervalEnd < info.IntervalStart || info.IntervalEnd < IntervalStart)
            if (!IsIntervalIntersect(info))
                return false;
            var maxEnd = new []{IntervalEnd, info.IntervalEnd}.Max();
            var minStart = new[] { IntervalStart, info.IntervalStart }.Min();
            gluedProductInfo = new ProductInfo(WarehouseId, Type, minStart, maxEnd);
            return true;
        }
        

        

        public override string ToString()
        {
            return $"{WarehouseId},{IntervalStart.ToString(DateFormat)} {IntervalEnd.ToString(DateFormat)},{Type}";
        }
    }
    class ProductInfos
    {
        private List<ProductInfo> Values { get; }

        public ProductInfos(IEnumerable<ProductInfo> infos)
        {
            Values = UniteIntersects(CorrectInfoNullType(infos)); //CorrectInfoNullType(infos);// UniteIntersects(CorrectInfoNullType(infos));
        }
        private bool TryUniteIntervalIntersectsWith(ProductInfo info, List<ProductInfo> infos, out ProductInfo gluedProductInfo)
        {
            gluedProductInfo = null;
            var thisInfo = info;
            foreach (var additionalInfo in infos)
            {
                if (thisInfo.TryUniteIntervalIntersectsWith(additionalInfo, out var tempGluedProductInfo))
                {
                    thisInfo = tempGluedProductInfo;
                }
            }
            gluedProductInfo = thisInfo;
            return true;
        }
        private List<ProductInfo> UniteIntersects(List<ProductInfo> infos)
        {
            var possibleIntersects = infos.GroupBy(v => (v.Type, v.WarehouseId))
                .ToDictionary(v => (v.Key.WarehouseId * 10 + (int)v.Key.Type), v => v.ToList());
            //Values.GroupBy(v => (v.Type, v.WarehouseId))
            var infosWithoutIntersects = infos
                .Select(info => TryUniteIntervalIntersectsWith(info, possibleIntersects[info.WarehouseId * 10 + ((int)info.Type)] /*infos*/, out var gluedProductInfo)
                    ? gluedProductInfo
                    : info)
                //.Distinct()
                .GroupBy(info =>new { info.Type, info.IntervalEnd, info.IntervalStart, info.WarehouseId })
                .Select(g => g.First())
                .ToList();

            /*
            infosWithoutIntersects = infosWithoutIntersects.GroupBy(info =>
                    new { info.Type, info.IntervalEnd, info.IntervalStart, info.WarehouseId })
                .Select(g => g.First())
                .ToList();
            */
            return infosWithoutIntersects;
        }
        private static List<ProductInfo> CorrectInfoNullType(IEnumerable<ProductInfo> infos)
        {
            var correctInfos = new List<ProductInfo>();
            foreach (var info in infos)
            {
                if (info.Type == ProductType.NULL)
                {
                    correctInfos.AddRange(info.CorrectInfoNullType());
                }
                else
                {
                    correctInfos.Add(info);
                }
            }
            return correctInfos;
        }

        public void Sort()
        {
            Values.Sort((info1, info2) =>
            {
                var warehouseIdDx = info1.WarehouseId - info2.WarehouseId;
                if (warehouseIdDx != 0) return warehouseIdDx;
                var typeDx = info1.Type - info2.Type;
                if (typeDx != 0) return typeDx;
                var startDx = info1.IntervalStart - info2.IntervalStart;
                return (int)startDx.Ticks;
            });
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Values.Select(info => info.ToString()));
        }
    }
    class TaskE
    {
        public static async Task Solve()
        {
            const string filePath = "input.txt";
            var fileContent = await File.ReadAllLinesAsync(filePath);
            var productInfos = new ProductInfos(fileContent.Select(ProductInfo.FromString));
            productInfos.Sort();
            Console.WriteLine(productInfos.ToString());
        }
    }
}
