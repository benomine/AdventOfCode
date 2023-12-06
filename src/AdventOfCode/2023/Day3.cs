namespace AdventOfCode._2023;

public partial class Day3 : IDay
{
    internal record Point(int Begin, int End, int Row, string Value)
    {
        public bool IsNumber => int.TryParse(Value, out _);
    };

    [GeneratedRegex(@"(\d+|[^\.\d\n])", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex Numbers();

    private static readonly Regex _numbersRegex = Numbers();

    public DateTime Date => new(2023,12,03,0,0,0,DateTimeKind.Local);

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var points = new List<Point>();
        var total = 0;
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        
        for (int i = 0; i < lines.Length; i++)
        {
            MatchCollection matches = _numbersRegex.Matches(lines[i]);
            foreach (Match match in matches)
            {
                points.Add(new(match.Index, match.Index + match.Length, i, match.Value));
            }
        }

        var specials = points.Where(x => !x.IsNumber).ToArray();
        var numbers = points.Where(x => x.IsNumber);
        foreach (var point in numbers)
        {
            if(specials.Any(x => 
                (x.Row == point.Row && (x.Begin == point.Begin - 1 || x.Begin == point.End)) ||
                (x.Row == point.Row + 1 && x.Begin >= point.Begin - 1 && x.Begin <= point.End) ||
                (x.Row == point.Row - 1 && x.Begin >= point.Begin - 1 && x.Begin <= point.End))
            )
            {
                total += int.Parse(point.Value);
            }
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var points = new List<Point>();
        var total = 0;
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        
        for (int i = 0; i < lines.Length; i++)
        {
            MatchCollection matches = _numbersRegex.Matches(lines[i]);
            foreach (Match match in matches)
            {
                points.Add(new(match.Index, match.Index + match.Length, i, match.Value));
            }
        }

        var gears = points.Where(x => !x.IsNumber && x.Value == "*");
        var numbers = points.Where(x => x.IsNumber);

        foreach (var gear in gears)
        {
            Point? firstValue = null;
            Point? secondValue = null;

            foreach (var item in numbers)
            {
                if((gear.Row == item.Row && (gear.Begin == item.Begin - 1 || gear.Begin == item.End)) ||
                    (gear.Row == item.Row + 1 && gear.Begin >= item.Begin - 1 && gear.Begin <= item.End) ||
                    (gear.Row == item.Row - 1 && gear.Begin >= item.Begin - 1 && gear.Begin <= item.End))
                {
                    if(firstValue is null)
                    {
                        firstValue = new Point(item.Begin, item.End, item.Row, item.Value);
                    }
                    else if (secondValue is null)
                    {
                        secondValue = new Point(item.Begin, item.End, item.Row, item.Value);
                    }
                }
                if(firstValue is not null && secondValue is not null)
                {
                    break;
                }
            }

            total += firstValue is not null && secondValue is not null ? int.Parse(firstValue.Value) * int.Parse(secondValue.Value) : 0;
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}
