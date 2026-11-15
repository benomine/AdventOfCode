using AdventOfCode._2015;

namespace AdventOfCode.Tests._2015;

public class Day1Tests : IDayTest
{
    private readonly IDay _sut = new Day1();

    [Test]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1("(())");

        Assert.Equals(result,"0");

        (result, _) = _sut.SolvePart1("(((");

        Assert.Equals(result,"3");

        (result, _) = _sut.SolvePart1("(()(()(");

        Assert.Equals(result,"3");
    }

    [Test]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(")");

        Assert.Equals(result,"1");

        (result, _) = _sut.SolvePart2("()())");

        Assert.Equals(result,"5");
    }
}