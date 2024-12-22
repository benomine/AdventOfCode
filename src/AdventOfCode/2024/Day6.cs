namespace AdventOfCode._2024
{
    public class Day6 : IDay
    {
        public DateTime Date => new(2024, 12, 06);
        public bool IsIgnored => false;

        private readonly HashSet<Point> _visited = new();
        private int _width = 0;
        private int _height = 0;
        private string[] _lines;
        private char[,] _map;
        private (int, int) _startingPosition;
        
        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var total = 0;

            _lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            _width = _lines[0].Length;
            _height = _lines.Length;

            _map = new char[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _map[x, y] = _lines[y][x];
                    if (_map[x, y] == '^')
                    {
                        _startingPosition = (x, y);
                    }
                }
            }

            CountSteps(_startingPosition);
            
            total = _visited.Count;

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var total = 0;

            var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        private void CountSteps(Point start)
        {
            var currentDirection = new Point(0, -1);
            var currentPoint = start;
            while (true)
            {
                _visited.Add(currentPoint);
                var nextPosition = currentPoint + currentDirection;
                if (IsOutOfBounds(nextPosition))
                {
                    break;
                }

                if (_map[nextPosition.X, nextPosition.Y] is '#')
                {
                    // Turn right
                    currentDirection = new Point(-currentDirection.Y, currentDirection.X);
                    nextPosition = currentPoint + currentDirection;
                }

                if (IsOutOfBounds(nextPosition))
                {
                    break;
                }

                currentPoint = nextPosition;
            }
        }
        
        private bool IsOutOfBounds(Point position)
        {
            return position.X < 0 || position.Y < 0 || position.X >= _width || position.Y >= _height;
        }

        public record struct Point(int X, int Y)
        {
            public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
    
            public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);

            public static Point operator *(Point point, int multiple) => new Point(point.X * multiple, point.Y * multiple);

            public Point Normalize() => new Point(X != 0 ? X / Math.Abs(X) : 0, Y != 0 ? Y / Math.Abs(Y) : 0);

            public static implicit operator Point((int X, int Y) tuple) => new Point(tuple.X, tuple.Y);

            public int ManhattanDistance(Point b) => Math.Abs(X - b.X) + Math.Abs(Y - b.Y);
        }
    }
}