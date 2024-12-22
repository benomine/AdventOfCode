using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day6Tests : IDayTest
    {
        private IDay _sut = new Day6();

        private const string _input =
            """
            ....#.....
            .........#
            ..........
            ..#.......
            .......#..
            ..........
            .#..^.....
            ........#.
            #.........
            ......#...
            """;

        private const string _result1 = "41";
        private const string _result2 = "";
        
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