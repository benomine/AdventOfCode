namespace AdventOfCode._2023;

public class Day10 : IDay
{
    public DateTime Date => new(2023, 12, 10, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;

    private class Position(int x, int y)
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;

        public Position((int x, int y) tuple) : this(tuple.x, tuple.y) {  }
    }
    
    private enum CardinalPoint
    {
        N,
        S,
        E,
        W
    }
    
    private CardinalPoint[] GetPossibleMovements(char value) =>
        value switch
        {
            '|' => [ CardinalPoint.N, CardinalPoint.S ],
            '-' => [ CardinalPoint.E, CardinalPoint.W ],
            'L' => [ CardinalPoint.N, CardinalPoint.E ],
            'J' => [ CardinalPoint.N, CardinalPoint.W ],
            '7' => [ CardinalPoint.S, CardinalPoint.W ],
            'F' => [ CardinalPoint.S, CardinalPoint.E ],
            _ => []
        };

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        
        
        return (0.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);


        return (0.ToString(), Stopwatch.GetElapsedTime(start));
    }
}