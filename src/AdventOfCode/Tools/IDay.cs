namespace AdventOfCode.Tools;

public interface IDay
{
    DateTime Date { get; }
    bool IsIgnored { get; }
    (string result, TimeSpan timeTaken) SolvePart1(string input);
    (string result, TimeSpan timeTaken) SolvePart2(string input);
}
