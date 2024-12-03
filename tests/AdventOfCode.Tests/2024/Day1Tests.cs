using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day1Tests : IDayTest
    {
        private readonly IDay _sut = new Day1();
        private const string _input = 
            """
            3   4
            4   3
            2   5
            1   3
            3   9
            3   3
            """;

        private const string _result1 = "11";
        private const string _result2 = "31";
        
        [Fact]
        public void SolvePart1()
        {
            var (result, _) = _sut.SolvePart1(_input);
        
            result.Should().Be(_result1);
        }
        
        [Fact]
        public void SolvePart2()
        {
            var (result, _) = _sut.SolvePart2(_input);
        
            result.Should().Be(_result2);
        }
    }
}