using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023;

public class Day1Tests : IDayTest
{
    private readonly IDay _sut = new Day1();
    private const string _input 
        = """
          1abc2
          pqr3stu8vwx
          a1b2c3d4e5f
          treb7uchet
          """;

    private const string _secondInput
        = """
          two1nine
          eightwothree
          abcone2threexyz
          xtwone3four
          4nineeightseven2
          zoneight234
          7pqrstsixteen
          """;
    
    [Fact]
    public void SolvePart1()
    {
        var (result, _) = _sut.SolvePart1(_input);
        
        result.Should().Be(142);
    }
    
    [Fact]
    public void SolvePart2()
    {
        var (result, _) = _sut.SolvePart2(_secondInput);
        
        result.Should().Be(281);
    }
}