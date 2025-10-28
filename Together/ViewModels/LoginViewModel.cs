using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        NavigateToRegisterCommand = new RelayCommand(_ => NavigateToRegister());
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ErrorMessage = string.Empty;
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ErrorMessage = string.Empty;
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }

    public event EventHandler? LoginSuccessful;
    public event EventHandler? NavigateToRegisterRequested;

    private bool CanLogin()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password);
    }

    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var loginDto = new LoginDto(Email, Password);
            var result = await _authenticationService.LoginAsync(loginDto);

            if (result.Success)
            {
                // Store token and user info (simplified - in production use secure storage)
                System.Windows.Application.Current.Properties["AuthToken"] = result.Token;
                System.Windows.Application.Current.Properties["CurrentUser"] = result.User;

                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NavigateToRegister()
    {
        NavigateToRegisterRequested?.Invoke(this, EventArgs.Empty);
    }
}
