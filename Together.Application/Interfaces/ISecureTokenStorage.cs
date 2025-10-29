namespace Together.Application.Interfaces;

/// <summary>
/// Service for securely storing and retrieving authentication tokens
/// </summary>
public interface ISecureTokenStorage
{
    /// <summary>
    /// Stores a token securely in Windows Credential Manager
    /// </summary>
    Task<bool> StoreTokenAsync(string key, string token);

    /// <summary>
    /// Retrieves a token from Windows Credential Manager
    /// </summary>
    Task<string?> RetrieveTokenAsync(string key);

    /// <summary>
    /// Deletes a token from Windows Credential Manager
    /// </summary>
    Task<bool> DeleteTokenAsync(string key);

    /// <summary>
    /// Checks if a token exists in storage
    /// </summary>
    Task<bool> TokenExistsAsync(string key);
}
