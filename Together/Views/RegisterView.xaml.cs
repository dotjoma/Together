using System.Windows;
using System.Windows.Controls;
using Together.Presentation.ViewModels;

namespace Together.Presentation.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ConfirmPassword = passwordBox.Password;
        }
    }
}
