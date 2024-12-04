using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day3Tests : IDayTest
    {
        private const string _input1 = 
            """
            xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)+mul(32,64]then(mul(11,8)mul(8,5))
            """;

        private const string _input2 =
            """
            xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))
            """;
        
        private const string _result1 = "161";
        private const string _result2 = "48";

        private readonly IDay _sut = new Day3();
        
        [Fact]
        public void SolvePart1()
        {
            var (result, _) = _sut.SolvePart1(_input1);
            
            result.Should().Be(_result1);
        }

        [Fact]
        public void SolvePart2()
        {
            var (result, _) = _sut.SolvePart2(_input2);
            
            result.Should().Be(_result2);
        }
    }
}