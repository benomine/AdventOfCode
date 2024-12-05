using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day4Tests : IDayTest
    {
        private readonly IDay _sut = new Day4();
        
        private const string _input =
            """
            MMMSXXMASM
            MSAMXMSMSA
            AMXSXMAAMM
            MSAMASMSMX
            XMASAMXAMM
            XXAMMXXAMA
            SMSMSASXSS
            SAXAMASAAA
            MAMMMXMMMM
            MXMXAXMASX
            """;

        private const string _result1 = "18";
        private const string _result2 = "9";
        
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