namespace AdventOfCode._2023;

public class Day6 : IDay
{
    public DateTime Date => new(2023, 12, 06, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var result = 1;
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var times = lines[0].Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        var distances = lines[1].Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        
        for (int i = 0; i < times.Length; i++)
        {
            result *= GetWays(times[i], distances[i]);
        }

        return (result.ToString(), Stopwatch.GetElapsedTime(start));
    }

    private static int GetWays(int time, int max)
    {
        var result = 0;
        for (int i = 1; i < time; i++)
        {
            if ((time - i) * i > max)
            {
                result++;
            }            
        }

        return result;
    }

    private static long GetWays(long time, long max)
    {
        var result = 0;
        for (int i = 1; i < time; i++)
        {
            if ((time - i) * i > max)
            {
                result++;
            }            
        }

        return result;
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var time = long.Parse(lines[0].Split(':')[1].Replace(" ", string.Empty));
        var distance = long.Parse(lines[1].Split(':')[1].Replace(" ", string.Empty));

        var result = GetWays(time, distance);

        return (result.ToString(), Stopwatch.GetElapsedTime(start));
    }
}