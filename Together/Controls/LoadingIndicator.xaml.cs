using System.Windows;
using System.Windows.Controls;

namespace Together.Presentation.Controls
{
    public partial class LoadingIndicator : UserControl
    {
        public static readonly DependencyProperty LoadingMessageProperty =
            DependencyProperty.Register(
                nameof(LoadingMessage),
                typeof(string),
                typeof(LoadingIndicator),
                new PropertyMetadata("Loading..."));

        public string LoadingMessage
        {
            get => (string)GetValue(LoadingMessageProperty);
            set => SetValue(LoadingMessageProperty, value);
        }

        public LoadingIndicator()
        {
            InitializeComponent();
        }
    }
}
