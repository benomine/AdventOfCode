namespace CoverageReporter.Core;

public static class CoverageClassGrouping
{
    public static string GetNamespace(string className, IReadOnlySet<string> knownClassNames)
    {
        ArgumentNullException.ThrowIfNull(className);
        ArgumentNullException.ThrowIfNull(knownClassNames);

        var enclosingClass = knownClassNames
            .Where(name => !string.Equals(name, className, StringComparison.Ordinal) &&
                           className.StartsWith(name + ".", StringComparison.Ordinal))
            .MaxBy(name => name.Length);
        if (enclosingClass is not null)
        {
            return GetNamespace(enclosingClass, knownClassNames);
        }

        var closureSuffix = className.IndexOf(".<>c", StringComparison.Ordinal);
        if (closureSuffix >= 0)
        {
            className = className[..closureSuffix];
        }

        var separator = className.LastIndexOf('.');
        return separator < 0 ? "(Global namespace)" : className[..separator];
    }

    public static bool IsCompilerGenerated(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.StartsWith("<>c", StringComparison.Ordinal) ||
               name.Contains(".<>c", StringComparison.Ordinal) ||
               name.StartsWith("System.Text.RegularExpressions.Generated.<RegexGenerator_g>", StringComparison.Ordinal);
    }
}