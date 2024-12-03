namespace AdventOfCode._2024
{
    public class Day1 : IDay
    {
        public DateTime Date => new(2024, 12, 1, 0, 0, 0, DateTimeKind.Local);
        public bool IsIgnored => false;
        
        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();

            List<int> list1 = []; 
            List<int> list2 = [];

            var total = 0;
            
            var lines = input.Split(Environment.NewLine);
            foreach (string line in lines)
            {
                var lineParts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                list1.Add(int.Parse(lineParts[0]));
                list2.Add(int.Parse(lineParts[1]));
            }
            
            list1.Sort();
            list2.Sort();

            for (int i = 0; i < list1.Count; i++)
            {
                total += Math.Abs(list1[i] - list2[i]);
            }
            
            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();

            List<int> list1 = []; 
            List<int> list2 = [];

            var total = 0;
            
            var lines = input.Split(Environment.NewLine);
            foreach (string line in lines)
            {
                var lineParts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                list1.Add(int.Parse(lineParts[0]));
                list2.Add(int.Parse(lineParts[1]));
            }

            foreach (var i in list1)
            {
                total += i * list2.Count(x => x == i);
            }
            
            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }
    }
}