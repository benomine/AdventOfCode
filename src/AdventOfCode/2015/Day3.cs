namespace AdventOfCode._2015;

public class Day3 : IDay
{
    public DateTime Date => new DateTime(2015, 12, 03, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var spans = input.ToCharArray();
        var total = spans.Count(x => x == '(') - spans.Count(x => x == ')');

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var spans = input.ToCharArray();
        var total = spans.Count(x => x == '(') - spans.Count(x => x == ')');

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}