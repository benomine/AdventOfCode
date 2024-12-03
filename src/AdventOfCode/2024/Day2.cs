namespace AdventOfCode._2024
{
    public class Day2 : IDay
    {
        public DateTime Date => new(2024, 12, 2);
        public bool IsIgnored => false;

        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();

            var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            var safe = 0;

            foreach (var line in lines)
            {
                var values = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

                if (IsLineSafe(values))
                {
                    safe++;
                }
            }

            return (safe.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();

            var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var safe = 0;

            foreach (string line in lines)
            {
                var values = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                if (IsLineSafe(values))
                {
                    safe++;
                }
                else
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        var innerValue = values.ToList();
                        innerValue.RemoveAt(i);
                        if (IsLineSafe(innerValue))
                        {
                            safe++;
                            break;
                        }
                    }
                }
            }

            return (safe.ToString(), Stopwatch.GetElapsedTime(start));
        }

        private static bool IsLineSafe(List<int> values)
        {
            if (values.Count < 2)
            {
                return true;
            }

            var firstDiff = values[1] - values[0];

            if (firstDiff == 0 || Math.Abs(firstDiff) > 3)
            {
                return false;
            }

            for (var i = 1; i < values.Count - 1; i++)
            {
                var diff = values[i] - values[i + 1];

                if (diff == 0 || Math.Abs(diff) > 3)
                {
                    return false;
                }

                var prevDiff = values[i - 1] - values[i];

                if ((int.IsNegative(diff) && int.IsPositive(prevDiff)) ||
                    (int.IsPositive(diff) && int.IsNegative(prevDiff)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}