namespace AdventOfCode._2023;

public class Day8 : IDay
{
    public DateTime Date => new(2023, 12, 08, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var instructions = lines[0].Select(x => x == 'L' ? 0 : 1).ToArray();
        var total = 0;

        var nodes = lines.Skip(2)
            .Select(x => x.Split(new[] { ' ', ',', '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(x => x[0], x => x[1..]);

        for (var node = "AAA"; node != "ZZZ"; total++)
        {
            node = nodes[node][instructions[total % instructions.Length]];
        }
        
        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        // not my solution, too tired to do it properly
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var instructions = lines[0].Select(x => x == 'L' ? 0 : 1).ToArray();

        var nodes = lines.Skip(2)
            .Select(x => x.Split(new[] { ' ', ',', '(', ')', '=' }, StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(x => x[0], x => x[1..]);

        var findloopFrequency = (string node) =>  // Scan until an end node is seen twice, first index is phase, index difference is period
        {
            var endSeen = new Dictionary<string, long>();
            for (long i = 0; true; i++)
            {
                if (node[2] == 'Z')
                {
                    if (endSeen.TryGetValue(node, out var lastSeen))
                        return (phase: lastSeen, period: i - lastSeen);
                    else
                        endSeen[node] = i;
                }
                node = nodes[node][instructions[i % instructions.Length]];
            }
        };

        var frequencies = 
            nodes.Keys
                .Where(x => x[2] == 'A')
                .Select(x => findloopFrequency(x))
                .ToList();

        // Find harmony by moving harmony phase forward and increasing harmony period until it matches all frequencies
        var harmonyPhase = frequencies[0].phase;
        var harmonyPeriod = frequencies[0].period;
        foreach (var freq in frequencies.Skip(1))
        {
            // Find new harmonyPhase by increasing phase in harmony period steps until harmony matches freq
            for (; harmonyPhase < freq.phase || (harmonyPhase - freq.phase) % freq.period != 0; harmonyPhase += harmonyPeriod) ;

            // Find the new harmonyPeriod by looking for the next position the harmony frequency matches freq (brute force least common multiplier)
            long sample = harmonyPhase + harmonyPeriod;
            for (; (sample - freq.phase) % freq.period != 0; sample += harmonyPeriod) ;
            harmonyPeriod = sample - harmonyPhase;
        }

        return (harmonyPhase.ToString(), Stopwatch.GetElapsedTime(start));
    }
}