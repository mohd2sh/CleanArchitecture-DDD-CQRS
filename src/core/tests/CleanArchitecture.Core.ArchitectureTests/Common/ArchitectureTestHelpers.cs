using System.Reflection;

namespace CleanArchitecture.Core.ArchitectureTests.Common;

/// <summary>
/// Helper methods for architecture tests.
/// </summary>
internal static class ArchitectureTestHelpers
{
    /// <summary>
    /// Extracts the bounded context name from a namespace.
    /// </summary>
    /// <param name="namespace">The namespace to extract from.</param>
    /// <param name="marker">The marker to look for (e.g., "Application" or "Domain").</param>
    /// <returns>The bounded context name, or empty string if not found.</returns>
    /// <example>
    /// ExtractBoundedContext("CleanArchitecture.Cmms.Application.Assets.Commands", "Application") returns "Assets"
    /// ExtractBoundedContext("CleanArchitecture.Cmms.Domain.Assets.Entities", "Domain") returns "Assets"
    /// </example>
    public static string ExtractBoundedContext(string? @namespace, string marker)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
            return string.Empty;

        var parts = @namespace.Split('.');
        var idx = Array.FindIndex(parts, p => p.Equals(marker, StringComparison.Ordinal));
        if (idx < 0 || idx + 1 >= parts.Length)
            return string.Empty;

        return parts[idx + 1]; // token immediately after Application/Domain
    }

    /// <summary>
    /// Checks if a property setter is an init-only setter.
    /// </summary>
    public static bool IsInitOnlySetter(PropertyInfo property)
    {
        if (property.SetMethod == null)
            return false;

        return property.SetMethod.ReturnParameter
            .GetRequiredCustomModifiers()
            .Any(m => m.Name == "IsExternalInit");
    }
}

