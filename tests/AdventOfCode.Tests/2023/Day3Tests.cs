using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023;

public class Day3Tests : IDayTest
{
    private readonly IDay _sut = new Day3();

    private const string _input
        = 
        """
        467..114..
        ...*......
        ..35..633.
        ......#...
        617*......
        .....+.58.
        ..592.....
        ......755.
        ...$.*....
        .664.598..
        """;

    [Fact]
    public void SolvePart1()
    {
        var expectedResult = "4361";

        var (result, _) = _sut.SolvePart1(_input);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void SolvePart2()
    {
        var expectedResult = "467835";
        
        var (result, _) = _sut.SolvePart2(_input);

        result.Should().Be(expectedResult);
    }
}
