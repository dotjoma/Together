using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Together.Application.Interfaces;

namespace Together.Application.Services;

/// <summary>
/// Service for managing application updates
/// Note: For .NET 8, ClickOnce deployment uses MSBuild-based publishing.
/// This service provides a framework for update checking that can be extended
/// with a custom update server or third-party update framework.
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly HttpClient _httpClient;
    private const string UpdateCheckUrl = ""; // Configure your update server URL here

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Checks if the application is deployed via ClickOnce
    /// For .NET 8, this checks for deployment markers
    /// </summary>
    public bool IsNetworkDeployed
    {
        get
        {
            // Check if running from a ClickOnce deployment
            // In .NET 8, we can check for specific deployment indicators
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var clickOnceMarker = System.IO.Path.Combine(appDataPath, "Together", ".clickonce");
            return System.IO.File.Exists(clickOnceMarker);
        }
    }

    /// <summary>
    /// Gets the current application version
    /// </summary>
    public Version CurrentVersion
    {
        get
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
        }
    }

    /// <summary>
    /// Checks for available updates asynchronously
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdateAsync()
    {
        _logger.LogInformation("Checking for application updates...");

        // If no update server is configured, return not available
        if (string.IsNullOrEmpty(UpdateCheckUrl))
        {
            _logger.LogInformation("Update server not configured. Update check skipped.");
            return new UpdateCheckResult
            {
                UpdateAvailable = false,
                Message = "Automatic updates are not configured for this installation."
            };
        }

        try
        {
            // Check for updates from your update server
            var response = await _httpClient.GetAsync($"{UpdateCheckUrl}/version.json");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to check for updates. Server returned: {StatusCode}", response.StatusCode);
                return new UpdateCheckResult
                {
                    UpdateAvailable = false,
                    Message = "Unable to check for updates at this time.",
                    Error = $"Server returned {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var versionInfo = JsonSerializer.Deserialize<VersionInfo>(content);

            if (versionInfo == null)
            {
                return new UpdateCheckResult
                {
                    UpdateAvailable = false,
                    Message = "Unable to parse update information."
                };
            }

            var availableVersion = new Version(versionInfo.Version);
            var currentVersion = CurrentVersion;

            if (availableVersion > currentVersion)
            {
                _logger.LogInformation("Update available. Current: {Current}, Available: {Available}", 
                    currentVersion, availableVersion);
                
                return new UpdateCheckResult
                {
                    UpdateAvailable = true,
                    AvailableVersion = availableVersion,
                    IsUpdateRequired = versionInfo.IsRequired,
                    UpdateSize = versionInfo.SizeBytes,
                    Message = versionInfo.IsRequired 
                        ? "A required update is available and must be installed." 
                        : "A new version is available.",
                    DownloadUrl = versionInfo.DownloadUrl
                };
            }

            _logger.LogInformation("No updates available. Current version is up to date.");
            return new UpdateCheckResult
            {
                UpdateAvailable = false,
                Message = "You are running the latest version"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to check for updates due to network error");
            return new UpdateCheckResult
            {
                UpdateAvailable = false,
                Message = "Failed to check for updates. Please check your internet connection.",
                Error = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during update check");
            return new UpdateCheckResult
            {
                UpdateAvailable = false,
                Message = "An unexpected error occurred while checking for updates.",
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Downloads and installs available updates
    /// </summary>
    public async Task<UpdateInstallResult> InstallUpdateAsync(IProgress<int>? progress = null)
    {
        _logger.LogInformation("Update installation requested");

        // For .NET 8 ClickOnce, updates are typically handled by the deployment infrastructure
        // This method provides a framework for custom update installation
        
        return await Task.FromResult(new UpdateInstallResult
        {
            Success = false,
            Message = "Automatic update installation is not yet configured. Please download the latest version manually.",
            RestartRequired = false
        });
    }

    /// <summary>
    /// Checks for updates on application startup
    /// </summary>
    public async Task CheckForUpdateOnStartupAsync()
    {
        try
        {
            var result = await CheckForUpdateAsync();
            
            if (result.UpdateAvailable)
            {
                _logger.LogInformation("Update available on startup: {Message}", result.Message);
                
                // For required updates, you could show a dialog here
                if (result.IsUpdateRequired)
                {
                    _logger.LogWarning("Required update detected but automatic installation not configured");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during startup update check");
        }
    }

    private class VersionInfo
    {
        public string Version { get; set; } = "1.0.0";
        public bool IsRequired { get; set; }
        public long SizeBytes { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
    }
}

/// <summary>
/// Result of an update check operation
/// </summary>
public class UpdateCheckResult
{
    public bool UpdateAvailable { get; set; }
    public Version? AvailableVersion { get; set; }
    public bool IsUpdateRequired { get; set; }
    public Version? MinimumRequiredVersion { get; set; }
    public long UpdateSize { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string? DownloadUrl { get; set; }
}

/// <summary>
/// Result of an update installation operation
/// </summary>
public class UpdateInstallResult
{
    public bool Success { get; set; }
    public bool RestartRequired { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
}
