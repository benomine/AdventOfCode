using System.Text;

namespace AdventOfCode._2023;

public class Day7 : IDay
{
    public DateTime Date => new(2023, 12, 07, 0, 0, 0, DateTimeKind.Local);
    public bool IsIgnored => false;

    private class Hand : IComparable<Hand>
    {
        private bool Joker { get; set; }
        private char[] Cards { get; set; }

        public int Bid { get; set; }

        private int GetStrength()
        {
            if (Joker)
            {
                var jokers = Cards.Count(x => x == 'J');
                var sb = new StringBuilder();
                foreach (char card in Cards)
                {
                    if (card != 'J')
                    {
                        sb.Append(card);
                    }
                }
                
                var jokerGroups =
                    sb.ToString()
                        .GroupBy(x => x)
                        .Select(x => x.Count())
                        .OrderByDescending(x => x)
                        .Concat(new[] {0})
                        .ToArray();
                jokerGroups[0] += jokers;
                return jokerGroups switch
                {
                    [5, ..] => 7,
                    [4, ..] => 6,
                    [3, 2, ..] => 5,
                    [3, ..] => 4,
                    [2, 2, ..] => 3,
                    [2, ..] => 2,
                    [..] => 1,
                };
            }
            
            var groups =
                Cards
                    .GroupBy(x => x)
                    .Select(x => x.Count())
                    .OrderByDescending(x => x)
                    .Concat(new[] {0})
                    .ToArray();
            return groups switch
            {
                [5, ..] => 7,
                [4, ..] => 6,
                [3, 2, ..] => 5,
                [3, ..] => 4,
                [2, 2, ..] => 3,
                [2, ..] => 2,
                [..] => 1,
            };
        }

        public Hand(string input, bool joker)
        {
            Joker = joker;
            Cards ??= input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].ToCharArray();
            Bid = int.Parse(input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        private int GetRank(char @char) => @char switch
        {
            'A' => 14,
            'K' => 13,
            'Q' => 12,
            'J' => Joker ? 1 : 11,
            'T' => 10,
            '9' => 9,
            '8' => 8,
            '7' => 7,
            '6' => 6,
            '5' => 5,
            '4' => 4,
            '3' => 3,
            '2' => 2,
            _ => throw new NotImplementedException()
        };
        
        public int CompareTo(Hand? other)
        {
            if (GetStrength() < other.GetStrength())
            {
                return -1;
            }

            if (GetStrength() > other.GetStrength())
            {
                return 1;
            }

            for (int i = 0; i < other.Cards.Length; i++)
            {
                if (other.Cards[i] == Cards[i])
                {
                    continue;
                }

                if (GetRank(other.Cards[i]) > GetRank(Cards[i]))
                {
                    return -1;
                }

                if (GetRank(other.Cards[i]) < GetRank(Cards[i]))
                {
                    return 1;
                }
            }

            return 0;
        }
    }

    public (string result, TimeSpan timeTaken) SolvePart1(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var hands = lines.Select(x => new Hand(x, false)).ToList();

        var result = hands.Order().Select((x, index) => x.Bid * (index + 1)).Sum();

        return (result.ToString(), Stopwatch.GetElapsedTime(start));
    }

    public (string result, TimeSpan timeTaken) SolvePart2(string input)
    {
        var start = Stopwatch.GetTimestamp();
        var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        var hands = lines.Select(x => new Hand(x, true)).ToList();

        var result = hands.Order().Select((x, index) => x.Bid * (index + 1)).Sum();

        return (result.ToString(), Stopwatch.GetElapsedTime(start));
    }
}