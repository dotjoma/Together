using System.Windows.Controls;
using Together.Presentation.ViewModels;

namespace Together.Presentation.Views;

public partial class ConnectionRequestView : UserControl
{
    public ConnectionRequestView()
    {
        InitializeComponent();
    }

    public ConnectionRequestView(ConnectionRequestViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
