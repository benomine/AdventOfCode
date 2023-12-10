using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023
{
    public class Day9Tests : IDayTest
    {
        private readonly IDay _sut = new Day9();

        private const string _input
            = """
              0 3 6 9 12 15
              1 3 6 10 15 21
              10 13 16 21 30 45
              """;
        
        [Fact]
        public void SolvePart1()
        {
            var (result, _) = _sut.SolvePart1(_input);

            result.Should().Be("68");
        }

        [Fact]
        public void SolvePart2()
        {
            var (result, _) = _sut.SolvePart2(_input);

            result.Should().Be("5");
        }
    }
}