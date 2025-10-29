using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace Together.Presentation.Controls
{
    public partial class EmptyStateView : UserControl
    {
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(
                nameof(IconKind),
                typeof(PackIconKind),
                typeof(EmptyStateView),
                new PropertyMetadata(PackIconKind.InboxArrowDown));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(EmptyStateView),
                new PropertyMetadata("No Items"));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(EmptyStateView),
                new PropertyMetadata("There are no items to display."));

        public static readonly DependencyProperty ActionButtonTextProperty =
            DependencyProperty.Register(
                nameof(ActionButtonText),
                typeof(string),
                typeof(EmptyStateView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ActionCommandProperty =
            DependencyProperty.Register(
                nameof(ActionCommand),
                typeof(ICommand),
                typeof(EmptyStateView),
                new PropertyMetadata(null));

        public PackIconKind IconKind
        {
            get => (PackIconKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string ActionButtonText
        {
            get => (string)GetValue(ActionButtonTextProperty);
            set => SetValue(ActionButtonTextProperty, value);
        }

        public ICommand ActionCommand
        {
            get => (ICommand)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        public EmptyStateView()
        {
            InitializeComponent();
        }
    }
}
