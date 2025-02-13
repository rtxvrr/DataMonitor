using Microsoft.Win32;

namespace DataMonitor.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string OpenFileDialog(string filter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter
            };
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
    }
}
