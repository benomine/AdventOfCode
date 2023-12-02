using FluentAssertions;

namespace AdventOfCode2023.Tests;

public class Day1Tests
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
    public void Parser_Should_Return_Correct_Sum()
    {
        var sum = _sut.SolvePart1(_input);
        
        sum.Should().Be("142");
    }
    
    [Fact]
    public void Parser_Should_Return_Correct_SecondSum()
    {
        var secondSum = _sut.SolvePart2(_secondInput);
        
        secondSum.Should().Be("281");
    }
}