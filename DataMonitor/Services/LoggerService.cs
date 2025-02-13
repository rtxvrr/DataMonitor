using DataMonitor.Data.Context;
using DataMonitor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.IO;

namespace DataMonitor.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly IDbContextFactory<DataMonitorContext> _contextFactory;
        private readonly ConcurrentDictionary<string, LogType> _logTypesCache = new();

        public LoggerService(IDbContextFactory<DataMonitorContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        private async Task<LogType> GetLogTypeAsync(string typeTitle)
        {
            if (_logTypesCache.TryGetValue(typeTitle, out var logType))
                return logType;

            using var context = _contextFactory.CreateDbContext();
            var types = await context.LogType.ToListAsync();
            logType = types.FirstOrDefault(t =>
                t.TypeTitle.Equals(typeTitle, StringComparison.OrdinalIgnoreCase));
            if (logType == null)
                throw new Exception($"LogType '{typeTitle}' не найден.");

            _logTypesCache.TryAdd(typeTitle, logType);
            return logType;
        }

        public async Task LogEventAsync(string message)
        {
            try
            {
                var logType = await GetLogTypeAsync("EVENT");
                using var context = _contextFactory.CreateDbContext();
                var log = new Log
                {
                    Message = message,
                    LogTypeID = logType.ID,
                    CreatedAt = DateTime.Now
                };
                await context.Log.AddAsync(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                WriteFallbackLog($"LogEventAsync failed. Message: {message}. Exception: {ex}");
            }
        }

        public async Task LogErrorAsync(string message)
        {
            try
            {
                var logType = await GetLogTypeAsync("ERROR");
                using var context = _contextFactory.CreateDbContext();
                var log = new Log
                {
                    Message = message,
                    LogTypeID = logType.ID,
                    CreatedAt = DateTime.Now
                };
                await context.Log.AddAsync(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                WriteFallbackLog($"LogErrorAsync failed. Message: {message}. Exception: {ex}");
            }
        }
        private void WriteFallbackLog(string logEntry)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FallbackLog.txt");
                File.AppendAllText(filePath, $"{DateTime.Now}: {logEntry}{Environment.NewLine}");
            }
            catch
            {
                // просто подавляю ошибку, если даже при создании файла с логом об ошибке возникнет ошибка, очень маловероятно
            }
        }
    }
}
