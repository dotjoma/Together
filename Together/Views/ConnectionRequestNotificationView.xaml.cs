using System.Windows.Controls;
using Together.Presentation.ViewModels;

namespace Together.Presentation.Views;

public partial class ConnectionRequestNotificationView : UserControl
{
    public ConnectionRequestNotificationView()
    {
        InitializeComponent();
    }

    public ConnectionRequestNotificationView(ConnectionRequestNotificationViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadPendingRequestsAsync();
    }
}
