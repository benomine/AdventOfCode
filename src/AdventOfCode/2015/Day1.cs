namespace AdventOfCode._2015;

public class Day1 : IDay
{
    public DateTime Date => new DateTime(2015, 12, 01, 0, 0, 0, DateTimeKind.Local);

    public (int result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var spans = input.ToCharArray();
        var total = spans.Count(x => x == '(') - spans.Count(x => x == ')');

        return (total, Stopwatch.GetElapsedTime(start));
    }

    public (int result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var spans = input.ToCharArray();
        var first = 0;
        var total = 0;
        for (var i = 0; i < spans.Length + 1; i++)
        {
            if (spans[i] is '(')
            {
                total += 1;
            }

            if (spans[i] is ')')
            {
                total -= 1;
                if (total < 0)
                {
                    first = i + 1;
                    break;
                }
            }
        }

        return (first, Stopwatch.GetElapsedTime(start));
    }
}