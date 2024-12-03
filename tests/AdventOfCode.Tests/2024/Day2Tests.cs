using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day2Tests : IDayTest
    {
        private IDay _sut = new Day2();
        
        private string _input =
            """
            7 6 4 2 1
            1 2 7 8 9
            9 7 6 2 1
            1 3 2 4 5
            8 6 4 4 1
            1 3 6 7 9
            """;

        private string _result1 = "2";
        private string _result2 = "4";
        
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