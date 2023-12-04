using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode._2015;

public class Day3 : IDay
{
    public DateTime Date => new DateTime(2015, 12, 03, 0, 0, 0, DateTimeKind.Local);

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
        var total = spans.Count(x => x == '(') - spans.Count(x => x == ')');

        return (total, Stopwatch.GetElapsedTime(start));
    }
}