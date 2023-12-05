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

var year = 2023;

var table = TableGenerator.Create(year);

var days = services.BuildServiceProvider().GetServices<IDay>().Where(x => x.Date.Year == year);
foreach (var day in days.OrderBy(x => x.Date))
{
    var input = File.ReadAllText(Path.Combine(year.ToString(), day.Date.ToString("yyyyMMdd") + ".txt"));
    var (result1, timeTaken1) = day.SolvePart1(input);
    var (result2, timeTaken2) = day.SolvePart2(input);
    table.AddRow(day.Date.ToString("d"), $"[green]{result1}[/]", $"[green]{timeTaken1}[/]", $"[red]{result2}[/]", $"[red]{timeTaken2}[/]");
}

AnsiConsole.Write(table);