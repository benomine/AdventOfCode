namespace AdventOfCode._2023;

public class Day9 : IDay
{
    public DateTime Date => new(2023, 12, 09, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;
    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var histories = 
            lines
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
                .ToList();
        var total = 0;
        
        foreach (int[] history in histories)
        {
            total += Extrapolate(history, true);
        }
        
        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    private static int Extrapolate(int[] report, bool forwards)
    {
        var initial = forwards
            ? report
            : report.Reverse();
        
        IList<int[]> sequences = new List<int[]> { initial.ToArray() };
        while (sequences[^1].Any(val => val != 0))
        {
            sequences.Add(item: sequences[^1]
                .Skip(1)
                .Select((val, i) => val - sequences[^1][i])
                .ToArray());
        }
        
        return sequences
            .Reverse()
            .Skip(1)
            .Aggregate(seed: 0, func: (n, seq) => n + seq[^1]);
    }
    
    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var histories = 
            lines
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
                .ToList();
        
        var total = 0;
        
        foreach (int[] history in histories)
        {
            total += Extrapolate(history, false);
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}