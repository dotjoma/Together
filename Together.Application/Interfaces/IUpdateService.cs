using System;
using System.Threading.Tasks;
using Together.Application.Services;

namespace Together.Application.Interfaces;

/// <summary>
/// Interface for application update management
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Indicates if the application is deployed via ClickOnce
    /// </summary>
    bool IsNetworkDeployed { get; }

    /// <summary>
    /// Gets the current application version
    /// </summary>
    Version CurrentVersion { get; }

    /// <summary>
    /// Checks for available updates
    /// </summary>
    Task<UpdateCheckResult> CheckForUpdateAsync();

    /// <summary>
    /// Downloads and installs available updates
    /// </summary>
    Task<UpdateInstallResult> InstallUpdateAsync(IProgress<int>? progress = null);

    /// <summary>
    /// Checks for updates on application startup
    /// </summary>
    Task CheckForUpdateOnStartupAsync();
}
