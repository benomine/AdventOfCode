using Spectre.Console;

namespace AdventOfCode.Tools;

public static class TableGenerator
{
    public static Table Create(int year)
    {
        var table = new Table
        {
            Title = new TableTitle($"Advent of Code {year}"),
            Border = TableBorder.Rounded
        };

        table.AddColumn("Day");
        table.AddColumn("Result 1");
        table.AddColumn("Time Taken 1");
        table.AddColumn("Result 2");
        table.AddColumn("Time Taken 2");

        return table;
    }
}
