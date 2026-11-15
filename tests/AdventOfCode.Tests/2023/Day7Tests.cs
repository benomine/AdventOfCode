using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023;

public class Day7Tests : IDayTest
{
    private readonly IDay _sut = new Day7();

    private const string _input
        = """
          32T3K 765
          T55J5 684
          KK677 28
          KTJJT 220
          QQQJA 483
          """;

    [Test]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1(_input);

        Assert.Equals(result,"6440");
    }

    [Test]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(_input);

        Assert.Equals(result,"5905");  
    }
}