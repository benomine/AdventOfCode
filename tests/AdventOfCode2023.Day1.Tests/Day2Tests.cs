using FluentAssertions;

namespace AdventOfCode2023.Tests;

public class Day2Tests : IDayTest
{
    private readonly IDay _sut = new Day2();

    private const string _input =
        """
        Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
        Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
        Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
        Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
        Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green
        """;

    [Fact]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1(_input);

        result.Should().Be("8");
    }

    [Fact]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(_input);

        result.Should().Be("2286");
    }
}
