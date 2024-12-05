namespace AdventOfCode._2024
{
    public class Day4 : IDay
    {
        private int _columns;
        private int _rows;
        private readonly char[] _word = ['M', 'A', 'S'];
        private string[] _lines = default!;

        public DateTime Date => new(2024, 12, 4);
        public bool IsIgnored => false;

        public (string result, TimeSpan timeTaken) SolvePart1(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var total = 0;

            _lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            _columns = _lines[0].Length;
            _rows = _lines.Length;

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    if (_lines[row][column] == 'X')
                    {
                        total += Find(row, column);
                    }
                }
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        public (string result, TimeSpan timeTaken) SolvePart2(string input)
        {
            var start = Stopwatch.GetTimestamp();
            var total = 0;

            _lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            _columns = _lines[0].Length;
            _rows = _lines.Length;

            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    if (_lines[row][column] == 'A')
                    {
                        total += FindX(row, column);
                    }
                }
            }

            return (total.ToString(), Stopwatch.GetElapsedTime(start));
        }

        private bool IsInvalidRow(int indexR) => indexR >= _rows || indexR < 0;
        private bool IsInvalidCol(int indexC) => indexC >= _columns || indexC < 0;

        private int FindInDirection(int r, int c, int dirR, int dirC)
        {
            for (int i = 0; i < _word.Length; i++)
            {
                var indexR = r + (i * dirR) + dirR;
                var indexC = c + (i * dirC) + dirC;

                if (IsInvalidRow(indexR) ||
                    IsInvalidCol(indexC) ||
                    _lines[indexR][indexC] != _word[i])
                {
                    return 0;
                }
            }

            return 1;
        }

        private int Find(int row, int column)
        {
            int[] directions = [-1, 0, 1];

            var result = 0;
            for (int directionRow = 0; directionRow < directions.Length; directionRow++)
            {
                for (int directionColumn = 0; directionColumn < directions.Length; directionColumn++)
                {
                    result += FindInDirection(row, column, directions[directionRow], directions[directionColumn]);
                }
            }

            return result;
        }

        private bool FindXChar(int row, int column, int directionRow, int directionColumn, char charToFind)
        {
            var indexRow = row + directionRow;
            var indexColumn = column + directionColumn;
            return !(IsInvalidRow(indexRow) ||
                     IsInvalidCol(indexColumn) ||
                     _lines[indexRow][indexColumn] != charToFind);
        }

        private bool FindXHalf(int row, int column, int directionRow, int directionColumn) =>
            FindXChar(row, column, directionRow, directionColumn, 'M') &&
            FindXChar(row, column, directionRow * -1, directionColumn * -1, 'S');

        private int FindX(int row, int column)
        {
            int[] dirs = [1, -1];

            var result = 0;
            for (int directionRow = 0; directionRow < dirs.Length; directionRow++)
            {
                for (int directionColumn = 0; directionColumn < dirs.Length; directionColumn++)
                {
                    result += FindXHalf(row, column, dirs[directionRow], dirs[directionColumn]) ? 1 : 0;
                }
            }

            return result == 2 ? 1 : 0;
        }
    }
}