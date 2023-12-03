using AdventOfCode2023;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spectre.Console;

var services = new ServiceCollection();

var rules = typeof(Program).Assembly.GetTypes()
    .Where(x => !x.IsAbstract && x.IsClass && x.GetInterface(nameof(IDay)) == typeof(IDay));

foreach (var rule in rules)
{
    services.Add(new ServiceDescriptor(typeof(IDay), rule, ServiceLifetime.Transient));
}

var table = new Table
{
    Title = new TableTitle($"Advent of Code 2023"),
    Border = TableBorder.Rounded
};

table.AddColumn("Day");
table.AddColumn("Result 1");
table.AddColumn("Time Taken 1");
table.AddColumn("Result 2");
table.AddColumn("Time Taken 2");

var days = services.BuildServiceProvider().GetServices<IDay>();
foreach (var day in days.OrderBy(x => x.Date))
{
    var input = File.ReadAllText(day.Date.ToString("yyyyMMdd") + ".txt");
    var (result1, timeTaken1) = day.SolvePart1(input);
    var (result2, timeTaken2) = day.SolvePart2(input);
    table.AddRow(day.Date.ToString("d"), $"[green]{result1}[/]", $"[green]{timeTaken1}[/]", $"[red]{result2}[/]", $"[red]{timeTaken2}[/]");
}

AnsiConsole.Write(table);