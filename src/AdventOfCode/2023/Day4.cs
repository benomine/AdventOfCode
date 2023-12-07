
namespace AdventOfCode._2023;

public class Day4 : IDay
{
    public DateTime Date => new(2023,12,04,0,0,0,DateTimeKind.Local);
    public bool IsIgnored => false;

    private sealed class Card
    {
        public int Value { get; set; }
        public int Total { get; set; }
        public int Wins { get; set; }
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var total = 0;
        using var reader = TextReader.Synchronized(new StringReader(input));

        var lines = input.Split(Environment.NewLine, StringSplitOptions.None);

        var cards = new Card?[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            cards[i] ??= new Card();
            cards[i]!.Total++;
            var split = lines[i].Split(':');

            var winnings = split[1].Split('|')[0].Trim().Split(' ');
            var yours = split[1].Split('|')[1].Trim().Split(' ');
            var wins = winnings.Where(x => !string.IsNullOrEmpty(x)).Intersect(yours.Where(x => !string.IsNullOrEmpty(x)));

            cards[i]!.Wins = wins.Count();

            cards[i]!.Value = wins.Count() switch
            {
                > 0 => (int)Math.Pow(2, wins.Count() - 1),
                _ => 0
            };

            for (int j = 1; j < wins.Count() + 1; j++)
            {
                if(i+j > lines.Length)
                {
                    break;
                }
                
                cards[i+j] ??= new Card();
                cards[i+j]!.Total += cards[i]!.Total;
            }
        }

        total = cards.Sum(c => c.Total);

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var total = 0;
        using var reader = TextReader.Synchronized(new StringReader(input));
        while (reader.ReadLine() is { } line)
        {
            var split = line.Split(':');
            var winnings = split[1].Split('|')[0].Trim().Split(' ');
            var yours = split[1].Split('|')[1].Trim().Split(' ');
            var wins = winnings.Where(x => !string.IsNullOrEmpty(x)).Intersect(yours.Where(x => !string.IsNullOrEmpty(x)));
            total += wins.Count() switch
            {
                > 0 => (int)Math.Pow(2, wins.Count() - 1),
                _ => 0
            };
        }

        return (total.ToString(), Stopwatch.GetElapsedTime(start));
    }
}
