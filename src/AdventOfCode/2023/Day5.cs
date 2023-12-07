namespace AdventOfCode._2023
{
    public class Day5 : IDay
    {
        public DateTime Date => new(2023, 12, 05, 0, 0, 0, DateTimeKind.Local);
        public bool IsIgnored => true;

        private record Ranges(long Source, long Target, long Range) : IComparable<Ranges>
        {
            private long Max => Source + Range;

            public int CompareTo(Ranges other)
            {
                long diff = Source - other.Source;
                return diff switch
                {
                    > 0 => 1,
                    0 => 0,
                    < 0 => -1
                };
            }

            public (bool isInRange, long newValue) IsIn(long value)
            {
                bool isInRange = value >= Source && value < Max;
                long newValue = isInRange ? Target + (value - Source) : -1;

                return (isInRange, newValue);
            }
        };

        private class Almanac
        {
            private readonly List<Ranges> _ranges = new();

            public void Add(long source, long target, long range) => _ranges.Add(new Ranges(source, target, range));

            public long TryGetValue(long value)
            {
                foreach (var range in _ranges.OrderBy(x => x.Source))
                {
                    var result = range.IsIn(value);
                    if (result.isInRange)
                    {
                        return result.newValue;
                    }
                }

                return value;
            }
        };

        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var result = long.MaxValue;
            var lines = input.Split(
                new[] { Environment.NewLine + Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            var seeds = lines[0].Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse);

            var seedToSoil = GetAlmanac(lines[1].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var soilToFertilizer = GetAlmanac(lines[2].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var fertilizerToWater = GetAlmanac(lines[3].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var waterToLight = GetAlmanac(lines[4].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var lightToTemperature = GetAlmanac(lines[5].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var temperatureToHumidity = GetAlmanac(lines[6].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var humidityToLocation = GetAlmanac(lines[7].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

            foreach (var seed in seeds)
            {
                var soil = seedToSoil.TryGetValue(seed);
                var fertilizer = soilToFertilizer.TryGetValue(soil);
                var water = fertilizerToWater.TryGetValue(fertilizer);
                var light = waterToLight.TryGetValue(water);
                var temperature = lightToTemperature.TryGetValue(light);
                var humidity = temperatureToHumidity.TryGetValue(temperature);
                var location = humidityToLocation.TryGetValue(humidity);

                result = Math.Min(location, result);
            }

            return (result.ToString(), Stopwatch.GetElapsedTime(start));
        }

        private static Almanac GetAlmanac(string[] lines)
        {
            var almanac = new Almanac();
            foreach (var line in lines)
            {
                var numbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
                almanac.Add(numbers[1],numbers[0],numbers[2]);
            }

            return almanac;
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();
            long? result = long.MaxValue;
            var lines = input.Split(
                new[] { Environment.NewLine + Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
           
            var chunks = 
                lines[0].Split(':')[1]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse).Chunk(2);

            var seedToSoil = GetAlmanac(lines[1].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var soilToFertilizer = GetAlmanac(lines[2].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var fertilizerToWater = GetAlmanac(lines[3].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var waterToLight = GetAlmanac(lines[4].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var lightToTemperature = GetAlmanac(lines[5].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var temperatureToHumidity = GetAlmanac(lines[6].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            var humidityToLocation = GetAlmanac(lines[7].Split(":")[1].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

            foreach (var chunk in chunks)
            {
                var seed = chunk[0];
                var range = chunk[1];
                for (long i = seed; i < seed + range; i++)
                {
                    var soil = seedToSoil.TryGetValue(i);
                    var fertilizer = soilToFertilizer.TryGetValue(soil);
                    var water = fertilizerToWater.TryGetValue(fertilizer);
                    var light = waterToLight.TryGetValue(water);
                    var temperature = lightToTemperature.TryGetValue(light);
                    var humidity = temperatureToHumidity.TryGetValue(temperature);
                    var location = humidityToLocation.TryGetValue(humidity);

                    result = Math.Min(result.Value, location);
                }
            }

            return (result!.Value.ToString(), Stopwatch.GetElapsedTime(start));
        }
    }
}