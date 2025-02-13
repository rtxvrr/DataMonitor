namespace DataMonitor.Services
{
    public interface ILoggerService
    {
        Task LogEventAsync(string message);
        Task LogErrorAsync(string message);
    }
}
