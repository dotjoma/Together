namespace Together.Application.Interfaces;

/// <summary>
/// Service for validating and sanitizing user input to prevent injection attacks
/// </summary>
public interface IInputValidator
{
    /// <summary>
    /// Sanitizes text input by removing potentially dangerous characters and scripts
    /// </summary>
    string SanitizeText(string input);

    /// <summary>
    /// Validates that input doesn't contain SQL injection patterns
    /// </summary>
    bool ContainsSqlInjectionPatterns(string input);

    /// <summary>
    /// Validates that input doesn't contain XSS patterns
    /// </summary>
    bool ContainsXssPatterns(string input);

    /// <summary>
    /// Sanitizes HTML content while preserving safe formatting
    /// </summary>
    string SanitizeHtml(string input);

    /// <summary>
    /// Validates file path to prevent directory traversal attacks
    /// </summary>
    bool IsValidFilePath(string path);

    /// <summary>
    /// Validates URL format and prevents malicious URLs
    /// </summary>
    bool IsValidUrl(string url);
}
