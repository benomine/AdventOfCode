using AdventOfCode._2015;

namespace AdventOfCode.Tests._2015;

public class Day1Tests : IDayTest
{
    private readonly IDay _sut = new Day1();

    [Fact]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1("(())");

        result.Should().Be(0);

        (result, _) = _sut.SolvePart1("(((");

        result.Should().Be(3);

        (result, _) = _sut.SolvePart1("(()(()(");

        result.Should().Be(3);
    }

    [Fact]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(")");

        result.Should().Be(1);

        (result, _) = _sut.SolvePart2("()())");

        result.Should().Be(5);
    }
}