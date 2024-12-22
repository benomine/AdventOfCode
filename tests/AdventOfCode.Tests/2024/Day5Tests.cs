using AdventOfCode._2024;

namespace AdventOfCode.Tests._2024
{
    public class Day5Tests : IDayTest
    {
        private const string _input =
            """
            47|53
            97|13
            97|61
            97|47
            75|29
            61|13
            75|53
            29|13
            97|29
            53|29
            61|53
            97|53
            61|29
            47|13
            75|47
            97|75
            47|61
            75|61
            47|29
            75|13
            53|13

            75,47,61,53,29
            97,61,53,29,13
            75,29,13
            75,97,47,61,53
            61,13,29
            97,13,75,29,47
            """;

        private const string _result1 = "143";
        private const string _result2 = "123";

        private IDay _sut = new Day5();
        
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