using AdventOfCode._2015;

namespace AdventOfCode.Tests._2015;

public class Day2Tests : IDayTest
{
    private readonly IDay _sut = new Day2();

    [Test]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1("2x3x4");

        Assert.Equals(result,"58");

        (result, _) = _sut.SolvePart1("1x1x10");

        Assert.Equals(result,"43");
    }

    [Test]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2("2x3x4");

        Assert.Equals(result,"34");

        (result, _) = _sut.SolvePart2("1x1x10");

        Assert.Equals(result,"14");
    }
}