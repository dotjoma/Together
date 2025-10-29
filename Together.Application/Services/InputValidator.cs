using System.Text.RegularExpressions;
using Together.Application.Interfaces;

namespace Together.Application.Services;

/// <summary>
/// Implementation of input validation and sanitization service
/// </summary>
public class InputValidator : IInputValidator
{
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|DECLARE|CAST|CONVERT)\b)|(-{2})|(/\*)|(\*/)|(\bOR\b.*=.*)|(\bAND\b.*=.*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script|javascript:|onerror=|onload=|<iframe|eval\(|expression\(|vbscript:|<object|<embed",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DirectoryTraversalPattern = new(
        @"\.\.|[<>:""|?*]|^[/\\]|[/\\]$",
        RegexOptions.Compiled);

    private static readonly Regex UrlPattern = new(
        @"^https?://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}(/.*)?$",
        RegexOptions.Compiled);

    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]*>",
        RegexOptions.Compiled);

    public string SanitizeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove null characters
        var sanitized = input.Replace("\0", string.Empty);

        // Remove control characters except newline, carriage return, and tab
        sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

        // Trim whitespace
        sanitized = sanitized.Trim();

        // Encode potentially dangerous characters
        sanitized = sanitized
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("/", "&#x2F;");

        return sanitized;
    }

    public bool ContainsSqlInjectionPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return SqlInjectionPattern.IsMatch(input);
    }

    public bool ContainsXssPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return XssPattern.IsMatch(input);
    }

    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // For now, remove all HTML tags
        // In a production app, you might want to use a library like HtmlSanitizer
        // to allow safe HTML tags while removing dangerous ones
        var sanitized = HtmlTagPattern.Replace(input, string.Empty);

        // Decode HTML entities to prevent double encoding
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

        // Re-encode to ensure safety
        sanitized = System.Net.WebUtility.HtmlEncode(sanitized);

        return sanitized;
    }

    public bool IsValidFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Check for directory traversal patterns
        if (DirectoryTraversalPattern.IsMatch(path))
        {
            return false;
        }

        // Check for absolute paths (should be relative)
        if (Path.IsPathRooted(path))
        {
            return false;
        }

        return true;
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Must be HTTP or HTTPS
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check against pattern
        if (!UrlPattern.IsMatch(url))
        {
            return false;
        }

        // Try to parse as URI
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        // Ensure it's HTTP or HTTPS scheme
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
