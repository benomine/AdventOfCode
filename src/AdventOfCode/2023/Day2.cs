namespace AdventOfCode._2023;

public partial class Day2 : IDay
{
    public DateTime Date => new(2023, 12, 02, 0, 0, 0, DateTimeKind.Local);
    private const int _maxRed = 12;
    private const int _maxGreen = 13;
    private const int _maxBlue = 14;

    private const string _red = "red";
    private const string _green = "green";
    private const string _blue = "blue";

    [GeneratedRegex(@"(\d+):(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GamesRegex();

    private readonly static Regex _gamesRegex = GamesRegex();

    private readonly Func<string, int> _action1 = (line) => {
        var result = _gamesRegex.Match(line);
        var gameNumber = int.Parse(result.Groups[1].Value);
        var games = result.Groups[2].Value.Trim().Split(';');
        for (int i = 0; i < games.Length; i++)
        {
            var possibilities = games[i].Split(',');
            for (int j = 0; j < possibilities.Length; j++)
            {
                if (possibilities[j].Contains(_red) && int.Parse(possibilities[j].Trim().Replace(_red, "")) > _maxRed
                || possibilities[j].Contains(_green) && int.Parse(possibilities[j].Trim().Replace(_green, "")) > _maxGreen
                || possibilities[j].Contains(_blue) && int.Parse(possibilities[j].Trim().Replace(_blue, "")) > _maxBlue)
                {
                    return 0;
                }
            }
        }

        return gameNumber;
    };

    private readonly Func<string, int> _action2 = (line) => {
        var maxRed = 0;
        var maxGreen = 0;
        var maxBlue = 0;
        var result = _gamesRegex.Match(line);
        var games = result.Groups[2].Value.Trim().Split(';');
        for (int i = 0; i < games.Length; i++)
        {
            var possibilities = games[i].Split(',');
            for (int j = 0; j < possibilities.Length; j++)
            {
                if (possibilities[j].Contains(_red) && int.Parse(possibilities[j].Trim().Replace(_red, "")) > maxRed)
                {
                    maxRed = int.Parse(possibilities[j].Trim().Replace(_red, ""));
                }
                if(possibilities[j].Contains(_green) && int.Parse(possibilities[j].Trim().Replace(_green, "")) > maxGreen)
                {
                    maxGreen = int.Parse(possibilities[j].Trim().Replace(_green, ""));
                } 
                if(possibilities[j].Contains(_blue) && int.Parse(possibilities[j].Trim().Replace(_blue, "")) > maxBlue)
                {
                    maxBlue = int.Parse(possibilities[j].Trim().Replace(_blue, ""));
                }
            }
        }

        return maxBlue * maxGreen * maxRed;
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
