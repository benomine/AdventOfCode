using AdventOfCode._2023;

namespace AdventOfCode.Tests._2023
{
    public class Day8Tests : IDayTest
    {
        private readonly IDay _sut = new Day8();

        private const string _input
            = """
              RL
              
              AAA = (BBB, CCC)
              BBB = (DDD, EEE)
              CCC = (ZZZ, GGG)
              DDD = (DDD, DDD)
              EEE = (EEE, EEE)
              GGG = (GGG, GGG)
              ZZZ = (ZZZ, ZZZ)
              """;
        
        private const string _input2
            = """
              LR
              
              11A = (11B, XXX)
              11B = (XXX, 11Z)
              11Z = (11B, XXX)
              22A = (22B, XXX)
              22B = (22C, 22C)
              22C = (22Z, 22Z)
              22Z = (22B, 22B)
              XXX = (XXX, XXX)
              """;

        [Fact]
        public void SolvePart1()
        {
            var (result, _) = _sut.SolvePart1(_input);

            result.Should().Be("2");
        }

        [Fact]
        public void SolvePart2()
        {
            var (result, _) = _sut.SolvePart2(_input2);

            result.Should().Be("6");
        }
    }
}