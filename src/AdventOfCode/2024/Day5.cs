namespace AdventOfCode._2024
{
    public class Day5 : IDay
    {
        public DateTime Date => new(2024, 12, 5);
        public bool IsIgnored => true;

        private HashSet<Rule> _rules = new();
        private List<Update> _updates = new();
        private HashSet<(int, int)> _hashRules = new();

        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();

            var total = 0;

            var lines = input.Split(Environment.NewLine);
            foreach (string line in lines)
            {
                if (line.Contains('|'))
                {
                    var parts = line.Split('|');
                    _rules.Add(new Rule(int.Parse(parts[0]), int.Parse(parts[1])));
                }

                if (line.Contains(','))
                {
                    var parts = line.Split(',');
                    _updates.Add(new Update(parts.Select(int.Parse).ToList()));
                }
            }

            foreach (Update update in _updates)
            {
                if (IsUpdateLegal(update))
                {
                    total += update.Pages[update.Pages.Count / 2];
                }
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();

            var total = 0;

            var lines = input.Split(Environment.NewLine);
            var rr = Split(lines[0], '|')
                .Select(t => (t[0], t[1]))
                .ToHashSet();

            foreach (var u in Split(lines[1], ','))
            {
                
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
            
            IEnumerable<int[]> Split(string s, char c) =>
                s.Split('\n').Select(t =>
                    t.Split(c).Select(int.Parse).ToArray());
        }

        private int Reorder(Update update)
        {
            var pages = update.Pages.ToList();
            pages.Sort((x, y) => _hashRules.Contains((y, x)) ? -1 : 1);

            return pages[pages.Count / 2];
        }

        private bool IsUpdateLegal(Update update)
        {
            for (int index = 0; index < update.Pages.Count - 1; index++)
            {
                if (!_rules.Any(r => r.Before == update.Pages[index] && r.After == update.Pages[index + 1]))
                {
                    return false;
                }
            }

            return true;
        }

        private record Rule(int Before, int After);

        private record Update(List<int> Pages);
    }
}