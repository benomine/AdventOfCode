using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023;

public class Day10Tests : IDayTest
{
    private readonly IDay _sut = new Day10();
    
    private const string _firstInput
        = """
          .....
          .S-7.
          .|.|.
          .L-J.
          .....
          """;

    private const string _secondInput
        = """
          ..F7.
          .FJ|.
          SJ.L7
          |F--J
          LJ...
          """;
    
    [Fact]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1(_firstInput);

        result.Should().Be("4");

        (result, _) = _sut.SolvePart1(_secondInput);

        result.Should().Be("8");
    }

    [Fact]
    public void SolvePart2()
    {
        
    }
}