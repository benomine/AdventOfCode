using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023;

public class Day6Tests : IDayTest
{
    private readonly IDay _sut = new Day6();

    private const string _input
        = """
        Time:      7  15   30
        Distance:  9  40  200
        """;

    [Fact]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1(_input);

        result.Should().Be("288");
    }

    [Fact]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(_input);

        result.Should().Be("71503");        
    }
}
