using System.Text.RegularExpressions;

namespace AdventOfCode2023;

public partial class Day1 : IDay
{
    private readonly static Regex _regex =
        new(@"(one|two|three|four|five|six|seven|eight|nine|zero|\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly static Regex _secondRegex =
        new(@"(one|two|three|four|five|six|seven|eight|nine|zero|\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);

    private readonly static Dictionary<string, int> _digits = new()
    {
        { "one", 1 },
        { "two", 2 },
        { "three", 3 },
        { "four", 4 },
        { "five", 5 },
        { "six", 6 },
        { "seven", 7 },
        { "eight", 8 },
        { "nine", 9 }
    };

    public DateTime Date => new(2023, 12, 1);

    private readonly Func<string, int> _action1 = (line) => {
        var digits = line.Where(char.IsDigit).Select(c => c - '0').ToArray();
        if (digits.Length == 0)
        {
            return 0;
        }
        return int.Parse(string.Join("", digits[0], digits[^1]));
    };

    private readonly Func<string, int> _action2 = (line) => {
        var match = _regex.Matches(line).Select(m => m.Value).First();

        var first = match.Length switch {
            1 => match[0] - '0',
            _ => _digits[match]
        };

        var secondMatch = _secondRegex.Matches(line).Select(m => m.Value).First();

        var last = secondMatch.Length switch {
            1 => secondMatch[0] - '0',
            _ => _digits[secondMatch]
        };

        return int.Parse(string.Join("", first, last));
    };

    private static IEnumerable<int> GetValues(string input, Func<string, int> action)
    {
        using var reader = TextReader.Synchronized(new StringReader(input));
        while (reader.ReadLine() is { } line)
        {
            yield return action(line);
        }
    }
    
    public string SolvePart1(string input)
    {
        var digits = GetValues(input, _action1);

        return digits.Sum().ToString();
    }

    public string SolvePart2(string input)
    {
        var digits = GetValues(input, _action2);

        return digits.Sum().ToString();
    }
}