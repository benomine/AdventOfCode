namespace AdventOfCode._2015;

public class Day2 : IDay
{
    public DateTime Date => new DateTime(2015, 12, 02, 0, 0, 0, DateTimeKind.Local);

    private readonly record struct Gift(int X, int Y, int Z)
    {
        public int Paper => 2 * X * Y + 2 * X * Z + 2 * Y * Z + Math.Min(Math.Min(X * Y, X * Z), Y * Z);

        public int Ribbon
        {
            get
            {
                var sizes = (new int[3] { X, Y, Z }).Order().ToArray();
                return X * Y * Z + sizes[0] + sizes[0] + sizes[1] + sizes[1];
            }
        }
    }

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var total = 0;
        using var reader = TextReader.Synchronized(new StringReader(input));
        while (reader.ReadLine() is { } line)
        {
            var result = line.Split('x');
            var cube = new Gift(int.Parse(result[0]), int.Parse(result[1]), int.Parse(result[2]));
            total += cube.Paper;
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
            var result = line.Split('x');
            var cube = new Gift(int.Parse(result[0]), int.Parse(result[1]), int.Parse(result[2]));
            total += cube.Ribbon;
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}