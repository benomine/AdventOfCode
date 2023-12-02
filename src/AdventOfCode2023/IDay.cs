namespace AdventOfCode2023;

public interface IDay
{
    DateTime Date { get;}
    string SolvePart1(string input);
    string SolvePart2(string input);
}
