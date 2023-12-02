namespace AdventOfCode2023;

public interface IDay
{
    DateTime Date { get;}
    (string result, TimeSpan timeTaken) SolvePart1(string input);
    (string result, TimeSpan timeTaken) SolvePart2(string input);
}
