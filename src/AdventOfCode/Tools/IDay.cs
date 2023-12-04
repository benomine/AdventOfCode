namespace AdventOfCode.Tools;

public interface IDay
{
    DateTime Date { get; }
    (int result, TimeSpan timeTaken) SolvePart1(string input);
    (int result, TimeSpan timeTaken) SolvePart2(string input);
}
