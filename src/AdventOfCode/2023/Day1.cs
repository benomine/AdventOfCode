namespace AdventOfCode._2023;

public partial class Day1 : IDay
{
    [GeneratedRegex(@"(one|two|three|four|five|six|seven|eight|nine|zero|\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex LeftToRight();
    [GeneratedRegex(@"(one|two|three|four|five|six|seven|eight|nine|zero|\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant)]
    private static partial Regex RightToLeft();

    private readonly static Regex _regex = LeftToRight();
    private readonly static Regex _secondRegex = RightToLeft();

    private static int GetInt(string number) => number switch
    {
        "one" => 1,
        "two" => 2,
        "three" => 3,
        "four" => 4,
        "five" => 5,
        "six" => 6,
        "seven" => 7,
        "eight" => 8,
        "nine" => 9,
        _ => throw new NotSupportedException()
    };

    public DateTime Date => new(2023, 12, 1, 0, 0, 0, DateTimeKind.Local);

    private readonly Func<string, int> _action1 = (line) => {
        var digits = line.Where(char.IsDigit).ToArray();
        
        return int.Parse(string.Concat("", digits[0], digits[^1]));
    };

    private readonly Func<string, int> _action2 = (line) => {
        var match = _regex.Matches(line)[0];

        var first = match.Length switch {
            1 => match.ValueSpan[0] - '0',
            _ => GetInt(match.Value)
        };

        var secondMatch = _secondRegex.Matches(line)[0];

        var last = secondMatch.Length switch {
            1 => secondMatch.ValueSpan[0] - '0',
            _ => GetInt(secondMatch.Value)
        };

        return int.Parse(string.Join("", first, last));
    };

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var total = 0;
        using var reader = TextReader.Synchronized(new StringReader(input));
        while (reader.ReadLine() is { } line)
        {
            total += _action1(line);
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var total = 0;
        using var reader = TextReader.Synchronized(new StringReader(input));
        while (reader.ReadLine() is { } line)
        {
            total += _action2(line);
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}