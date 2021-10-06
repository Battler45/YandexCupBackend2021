using System;
using System.Collections.Generic;
using System.Linq;

namespace YandexCupBackend
{
    enum Turn
    {
        White, Black
    }
    class Checkers
    {
        public Turn Turn { get; }
        public int Width { get; }
        public int Height { get; }
        public List<(int, int)> Whites { get; }
        public List<(int, int)> Blacks { get; }
        public Checkers(Turn turn, int width, int height, List<(int, int)> whites, List<(int, int)> blacks)
        {
            Turn = turn;
            Width = width;
            Height = height;
            Whites = whites;
            Blacks = blacks;
        }
        private static bool IsNeighboringChecker((int, int) checker, (int, int) possibleNeighbor)
        {
            return checker.Item1 == possibleNeighbor.Item1 - 1 && checker.Item2 == possibleNeighbor.Item2 - 1
                   || checker.Item1 == possibleNeighbor.Item1 + 1 && checker.Item2 == possibleNeighbor.Item2 + 1
                   || checker.Item1 == possibleNeighbor.Item1 - 1 && checker.Item2 == possibleNeighbor.Item2 + 1
                   || checker.Item1 == possibleNeighbor.Item1 + 1 && checker.Item2 == possibleNeighbor.Item2 - 1
                ;
        }
        private bool IsEmpty((int, int) position)
        {
            return !Whites.Contains(position) && !Blacks.Contains(position);
        }
        private bool IsInDesk((int, int) position)
        {
            return position.Item1 <= Width && position.Item2 <= Height
                && position.Item1 > 0 && position.Item2 > 0;
        }
        private bool CanBeat((int, int) checker, (int, int) possibleTarget)
        {
            var (dx, dy) = (possibleTarget.Item1 - checker.Item1, possibleTarget.Item2 - checker.Item2);
            var nextCheckerPosition = (possibleTarget.Item1 + dx, possibleTarget.Item2 + dy);
            return IsInDesk(nextCheckerPosition) && IsEmpty(nextCheckerPosition);
        }
        /*
        private List<(int, int)> GetCheckerTargets((int, int)checker, List<(int, int)> enemyCheckers)
        {
            var targets = enemyCheckers.Where(enemyChecker => Checkers.IsNeighboringChecker(checker, enemyChecker))
                .Where(enemyChecker => CanBeat(checker, enemyChecker))
                .ToList();
            return targets;
        }

        private List<(int, int)> GetTargets()
        {
            var (checkers, enemyCheckers) = (this.Turn == Turn.White)
                    ? (Whites, Blacks)
                    : (Blacks, Whites)
                ;
            var targets = checkers.Select(checker => GetCheckerTargets(checker, enemyCheckers))
                .Aggregate(new List<(int, int)>(), (list1, list2) =>
                {
                    list1.AddRange(list2);
                    return list1;
                });
            return targets;
        }
        */
        private bool HasCheckerTarget((int, int) checker, List<(int, int)> enemyCheckers)
        {
            var hasTargets = enemyCheckers
                    .Where(enemyChecker => IsNeighboringChecker(checker, enemyChecker))
                    .Any(enemyChecker => CanBeat(checker, enemyChecker))
                ;
            return hasTargets;
        }
        private bool HasTarget()
        {
            var (checkers, enemyCheckers) = (this.Turn == Turn.White)
                    ? (Whites, Blacks)
                    : (Blacks, Whites)
                ;
            var hasTarget = checkers.Any(checker => HasCheckerTarget(checker, enemyCheckers));
            return hasTarget;
        }
        public bool CanBeat()
        {
            return HasTarget(); //GetTargets().Any();
        }
    }

    class CheckersReader
    {
        private (int, int) ReadIntPair()
        {
            var pair = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            return (pair[0], pair[1]);
        }
        private List<(int, int)> ReadIntPairs(int count)
        {
            var pairs = new List<(int, int)>();
            for (var i = 0; i < count; i++)
            {
                pairs.Add(ReadIntPair());
            }

            return pairs;
        }
        private List<(int, int)> ReadCheckersPositions()
        {
            var positionsCount = int.Parse(Console.ReadLine());
            return ReadIntPairs(positionsCount);
        }
        private Turn ReadTurn()
        {
            Enum.TryParse(Console.ReadLine(), true, out Turn turn);
            return turn;
        }

        public Checkers ReadCheckers()
        {
            var (width, height) = ReadIntPair();
            var whites = ReadCheckersPositions();
            var blacks = ReadCheckersPositions();
            var turn = ReadTurn();
            var checkers = new Checkers(turn, width, height, whites, blacks);
            return checkers;
        }
    }
    class TaskC
    {
        public static void Solve()
        {
            var checkers = new CheckersReader().ReadCheckers();
            var canBeat = checkers.CanBeat();
            Console.WriteLine(canBeat ? "Yes" : "No");
        }
    }
}

/*
 *
8 8
2
7 7
5 5
1
6 6
white
 */