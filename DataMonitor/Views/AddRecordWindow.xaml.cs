using DataMonitor.ViewModels;
using System.Windows;

namespace DataMonitor.Views
{
    public partial class AddRecordWindow : Window
    {
        public AddRecordWindow(AddRecordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseAction = Close;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
