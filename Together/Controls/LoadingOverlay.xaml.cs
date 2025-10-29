using System.Windows;
using System.Windows.Controls;

namespace Together.Presentation.Controls
{
    public partial class LoadingOverlay : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                nameof(IsLoading),
                typeof(bool),
                typeof(LoadingOverlay),
                new PropertyMetadata(false));

        public static readonly DependencyProperty LoadingMessageProperty =
            DependencyProperty.Register(
                nameof(LoadingMessage),
                typeof(string),
                typeof(LoadingOverlay),
                new PropertyMetadata("Please wait..."));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string LoadingMessage
        {
            get => (string)GetValue(LoadingMessageProperty);
            set => SetValue(LoadingMessageProperty, value);
        }

        public LoadingOverlay()
        {
            InitializeComponent();
        }
    }
}
