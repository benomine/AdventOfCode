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
        
        [Test]
        public void SolvePart1()
        {
            var (result, _) = _sut.SolvePart1(_input);
        
            Assert.Equals(result, _result1);
        }
        
        [Test]
        public void SolvePart2()
        {
            var (result, _) = _sut.SolvePart2(_input);
        
            Assert.Equals(result, _result2);
        }
    }
}