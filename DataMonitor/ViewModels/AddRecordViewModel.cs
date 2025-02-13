using DataMonitor.Data.Context;
using DataMonitor.Data.Entities;
using DataMonitor.Helpers;
using DataMonitor.Messaging;
using DataMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Notifications.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DataMonitor.ViewModels
{
    public class AddRecordViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<DataMonitorContext> _contextFactory;
        private readonly ILoggerService _loggerService;

        public int? MachineNumber { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public DateTime TareDate { get; set; } = DateTime.Now;
        public DateTime GrossDate { get; set; } = DateTime.Now;
        public ObservableCollection<int> ExistingMachineNumbers { get; } = new();
        public ICommand AddRecordCommand { get; }
        public Action CloseAction { get; set; }

        public AddRecordViewModel(IDbContextFactory<DataMonitorContext> contextFactory, ILoggerService loggerService)
        {
            _contextFactory = contextFactory;
            _loggerService = loggerService;
            AddRecordCommand = new RelayCommand(async _ => await AddRecordAsync(), _ => MachineNumber.HasValue);
            _ = LoadExistingMachinesAsync();
        }

        private async Task LoadExistingMachinesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var machines = await context.Machine.ToListAsync();
                ExistingMachineNumbers.Clear();
                foreach (var machine in machines)
                    ExistingMachineNumbers.Add(machine.MachineNumber);
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Ошибка загрузки списка машин: {ex.Message}");
                MessageBox.Show("Ошибка загрузки списка машин. Попробуйте позже.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddRecordAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var machine = await context.Machine.FirstOrDefaultAsync(m => m.MachineNumber == MachineNumber.Value);
                if (machine == null)
                {
                    machine = new Machine { MachineNumber = MachineNumber.Value };
                    await context.Machine.AddAsync(machine);
                    await context.SaveChangesAsync();
                }

                var record = new MachineRecord
                {
                    MachineID = machine.ID,
                    GrossWeight = GrossWeight,
                    TareWeight = TareWeight,
                    NetWeight = NetWeight,
                    TareDate = TareDate,
                    GrossDate = GrossDate
                };
                await context.MachineRecord.AddAsync(record);
                await context.SaveChangesAsync();
                var notificationManager = new NotificationManager();
                notificationManager.Show(new NotificationContent
                {
                    Title = "Успех",
                    Message = "Запись успешно добавлена!",
                    Type = NotificationType.Success
                });
                await _loggerService.LogEventAsync("Новая запись успешно добавлена в базу данных.");
                Messenger.NotifyRecordAdded();

                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Ошибка добавления записи: {ex.Message}");
                MessageBox.Show($"Ошибка добавления записи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
