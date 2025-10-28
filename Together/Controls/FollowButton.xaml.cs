using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Together.Presentation.Commands;

namespace Together.Presentation.Controls;

public partial class FollowButton : UserControl
{
    public static readonly DependencyProperty FollowStatusProperty =
        DependencyProperty.Register(nameof(FollowStatus), typeof(string), typeof(FollowButton),
            new PropertyMetadata("none", OnFollowStatusChanged));

    public static readonly DependencyProperty FollowCommandProperty =
        DependencyProperty.Register(nameof(FollowCommand), typeof(ICommand), typeof(FollowButton));

    public string FollowStatus
    {
        get => (string)GetValue(FollowStatusProperty);
        set => SetValue(FollowStatusProperty, value);
    }

    public ICommand FollowCommand
    {
        get => (ICommand)GetValue(FollowCommandProperty);
        set => SetValue(FollowCommandProperty, value);
    }

    public string ButtonText { get; private set; } = "Follow";
    public PackIconKind IconKind { get; private set; } = PackIconKind.AccountPlus;

    public FollowButton()
    {
        InitializeComponent();
        DataContext = this;
        UpdateButtonAppearance();
    }

    private static void OnFollowStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FollowButton button)
        {
            button.UpdateButtonAppearance();
        }
    }

    private void UpdateButtonAppearance()
    {
        switch (FollowStatus)
        {
            case "none":
                ButtonText = "Follow";
                IconKind = PackIconKind.AccountPlus;
                break;
            case "pending":
                ButtonText = "Pending";
                IconKind = PackIconKind.Clock;
                break;
            case "accepted":
                ButtonText = "Following";
                IconKind = PackIconKind.AccountCheck;
                break;
            case "self":
                ButtonText = "You";
                IconKind = PackIconKind.Account;
                break;
            default:
                ButtonText = "Follow";
                IconKind = PackIconKind.AccountPlus;
                break;
        }
    }
}
