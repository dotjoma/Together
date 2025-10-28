using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _usernameError = string.Empty;
    private string _emailError = string.Empty;
    private string _passwordError = string.Empty;
    private string _confirmPasswordError = string.Empty;
    private bool _isLoading;

    public RegisterViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        RegisterCommand = new RelayCommand(async _ => await RegisterAsync(), _ => CanRegister());
        NavigateToLoginCommand = new RelayCommand(_ => NavigateToLogin());
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                UsernameError = string.Empty;
                ErrorMessage = string.Empty;
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                EmailError = string.Empty;
                ErrorMessage = string.Empty;
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
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
                PasswordError = string.Empty;
                ErrorMessage = string.Empty;
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                ConfirmPasswordError = string.Empty;
                ErrorMessage = string.Empty;
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string UsernameError
    {
        get => _usernameError;
        set => SetProperty(ref _usernameError, value);
    }

    public string EmailError
    {
        get => _emailError;
        set => SetProperty(ref _emailError, value);
    }

    public string PasswordError
    {
        get => _passwordError;
        set => SetProperty(ref _passwordError, value);
    }

    public string ConfirmPasswordError
    {
        get => _confirmPasswordError;
        set => SetProperty(ref _confirmPasswordError, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand NavigateToLoginCommand { get; }

    public event EventHandler? RegistrationSuccessful;
    public event EventHandler? NavigateToLoginRequested;

    private bool CanRegister()
    {
        return !IsLoading && 
               !string.IsNullOrWhiteSpace(Username) && 
               !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password) &&
               !string.IsNullOrWhiteSpace(ConfirmPassword);
    }

    private async Task RegisterAsync()
    {
        try
        {
            // Clear previous errors
            ClearErrors();

            // Validate confirm password
            if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match";
                return;
            }

            IsLoading = true;

            var registerDto = new RegisterDto(Username, Email, Password);
            var result = await _authenticationService.RegisterAsync(registerDto);

            if (result.Success)
            {
                // Store token and user info
                System.Windows.Application.Current.Properties["AuthToken"] = result.Token;
                System.Windows.Application.Current.Properties["CurrentUser"] = result.User;

                RegistrationSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (ValidationException ex)
        {
            // Display field-specific validation errors
            if (ex.Errors.TryGetValue("Username", out var usernameErrors))
            {
                UsernameError = string.Join(", ", usernameErrors);
            }
            if (ex.Errors.TryGetValue("Email", out var emailErrors))
            {
                EmailError = string.Join(", ", emailErrors);
            }
            if (ex.Errors.TryGetValue("Password", out var passwordErrors))
            {
                PasswordError = string.Join(", ", passwordErrors);
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

    private void ClearErrors()
    {
        UsernameError = string.Empty;
        EmailError = string.Empty;
        PasswordError = string.Empty;
        ConfirmPasswordError = string.Empty;
        ErrorMessage = string.Empty;
    }

    private void NavigateToLogin()
    {
        NavigateToLoginRequested?.Invoke(this, EventArgs.Empty);
    }
}
