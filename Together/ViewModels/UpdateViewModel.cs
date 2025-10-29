using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Application.Services;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

/// <summary>
/// ViewModel for application update management
/// </summary>
public class UpdateViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    private bool _isCheckingForUpdates;
    private bool _isInstallingUpdate;
    private bool _updateAvailable;
    private string _updateMessage = string.Empty;
    private string _currentVersion = string.Empty;
    private string _availableVersion = string.Empty;
    private int _downloadProgress;
    private bool _isUpdateRequired;

    public UpdateViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        _currentVersion = _updateService.CurrentVersion.ToString();

        CheckForUpdatesCommand = new RelayCommand(async _ => await CheckForUpdatesAsync(), _ => !IsCheckingForUpdates && !IsInstallingUpdate);
        InstallUpdateCommand = new RelayCommand(async _ => await InstallUpdateAsync(), _ => UpdateAvailable && !IsInstallingUpdate);
    }

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand InstallUpdateCommand { get; }

    public bool IsCheckingForUpdates
    {
        get => _isCheckingForUpdates;
        set => SetProperty(ref _isCheckingForUpdates, value);
    }

    public bool IsInstallingUpdate
    {
        get => _isInstallingUpdate;
        set => SetProperty(ref _isInstallingUpdate, value);
    }

    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => SetProperty(ref _updateAvailable, value);
    }

    public string UpdateMessage
    {
        get => _updateMessage;
        set => SetProperty(ref _updateMessage, value);
    }

    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value);
    }

    public string AvailableVersion
    {
        get => _availableVersion;
        set => SetProperty(ref _availableVersion, value);
    }

    public int DownloadProgress
    {
        get => _downloadProgress;
        set => SetProperty(ref _downloadProgress, value);
    }

    public bool IsUpdateRequired
    {
        get => _isUpdateRequired;
        set => SetProperty(ref _isUpdateRequired, value);
    }

    public bool IsNetworkDeployed => _updateService.IsNetworkDeployed;

    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;
        UpdateMessage = "Checking for updates...";

        try
        {
            var result = await _updateService.CheckForUpdateAsync();

            UpdateAvailable = result.UpdateAvailable;
            UpdateMessage = result.Message;
            IsUpdateRequired = result.IsUpdateRequired;

            if (result.UpdateAvailable && result.AvailableVersion != null)
            {
                AvailableVersion = result.AvailableVersion.ToString();
            }
        }
        catch (Exception ex)
        {
            UpdateMessage = $"Error checking for updates: {ex.Message}";
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    private async Task InstallUpdateAsync()
    {
        IsInstallingUpdate = true;
        UpdateMessage = "Downloading and installing update...";
        DownloadProgress = 0;

        try
        {
            var progress = new Progress<int>(percent =>
            {
                DownloadProgress = percent;
            });

            var result = await _updateService.InstallUpdateAsync(progress);

            UpdateMessage = result.Message;

            if (result.Success && result.RestartRequired)
            {
                UpdateMessage += " The application will restart to complete the update.";
            }
        }
        catch (Exception ex)
        {
            UpdateMessage = $"Error installing update: {ex.Message}";
        }
        finally
        {
            IsInstallingUpdate = false;
        }
    }

    public async Task CheckForUpdatesOnStartupAsync()
    {
        await _updateService.CheckForUpdateOnStartupAsync();
    }
}
