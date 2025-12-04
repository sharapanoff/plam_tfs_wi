using System.Text.RegularExpressions;

namespace TfsViewer.App.Services;

/// <summary>
/// Service for resolving parameterized strings with %PropertyName% syntax
/// </summary>
public static class ParameterResolver
{
    /// <summary>
    /// Resolves parameters in a template string using properties from the provided object.
    /// Supports %PropertyName% syntax (case-insensitive).
    /// Example: "Report for %ProjectName% in %State%" will be replaced with actual values.
    /// </summary>
    /// <param name="template">The template string with %PropertyName% parameters</param>
    /// <param name="source">The object to extract property values from</param>
    /// <returns>The resolved string with all parameters replaced</returns>
    public static string ResolveParameters(string? template, object? source)
    {
        if (string.IsNullOrEmpty(template) || source == null)
            return template ?? string.Empty;

        // Pattern to match %PropertyName% - allows letters, numbers, underscores
        var pattern = @"%([a-zA-Z_][a-zA-Z0-9_]*)%";
        
        return Regex.Replace(template, pattern, match =>
        {
            var propertyName = match.Groups[1].Value;
            var value = GetPropertyValue(source, propertyName);
            return value ?? match.Value; // Return original %PropertyName% if not found
        }, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Gets a property value from an object using reflection (case-insensitive).
    /// </summary>
    /// <param name="source">The object to get the property from</param>
    /// <param name="propertyName">The name of the property (case-insensitive)</param>
    /// <returns>The property value as a string, or null if not found</returns>
    private static string? GetPropertyValue(object source, string propertyName)
    {
        if (source == null || string.IsNullOrEmpty(propertyName))
            return null;

        try
        {
            // Get all properties with case-insensitive comparison
            var property = source.GetType()
                .GetProperties(System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                return null;

            var value = property.GetValue(source);
            
            // Handle different types
            if (value == null)
                return string.Empty;
            
            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            
            if (value is bool boolValue)
                return boolValue.ToString().ToLower();
            
            return value.ToString();
        }
        catch
        {
            // If any error occurs during reflection, return null
            return null;
        }
    }
}
