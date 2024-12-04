namespace AdventOfCode._2024
{
    public partial class Day3 : IDay
    {
        public DateTime Date => new DateTime(2024, 12, 3);
        public bool IsIgnored => false;

        [GeneratedRegex(@"mul\((?<left>\d{1,3})\,(?<right>\d{1,3})\)")]
        private static partial Regex MulRegex();

        [GeneratedRegex(@"do\(\)")]
        private static partial Regex DoRegex();

        [GeneratedRegex(@"don't\(\)")]
        private static partial Regex DontRegex();

        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var total = 0;

            var matches = MulRegex().Matches(input);
            foreach (Match match in matches)
            {
                total += int.Parse(match.Groups["left"].ValueSpan) * int.Parse(match.Groups["right"].ValueSpan);
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();

            var total = 0;

            var matches = MulRegex().Matches(input);
            var doMatches = DoRegex().Matches(input).Select(x => x.Index).ToList();
            var dontMatches = DontRegex().Matches(input).Select(x => x.Index).ToList();

            var merged = doMatches.Concat(dontMatches).OrderBy(x => x).ToArray();
            var next = 0;

            var isEnabled = true;

            foreach (Match match in matches)
            {
                var matchedIndex = match.Index;
                if (next < merged.Length && matchedIndex > merged[next])
                {
                    var isDo = doMatches.Contains(merged[next]);
                    isEnabled = isDo;
                    next++;
                }

                if (isEnabled)
                {
                    total += int.Parse(match.Groups["left"].ValueSpan) *
                             int.Parse(match.Groups["right"].ValueSpan);
                }
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }
    }
}