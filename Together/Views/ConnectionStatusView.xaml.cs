using System.Windows.Controls;
using Together.Presentation.ViewModels;

namespace Together.Presentation.Views;

public partial class ConnectionStatusView : UserControl
{
    public ConnectionStatusView()
    {
        InitializeComponent();
    }

    public ConnectionStatusView(ConnectionStatusViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
