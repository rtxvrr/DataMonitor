using DataMonitor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DataMonitor.Services
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void ShowAddRecordWindow()
        {
            var addRecordWindow = _serviceProvider.GetRequiredService<AddRecordWindow>();
            addRecordWindow.ShowDialog();
        }
    }
}
