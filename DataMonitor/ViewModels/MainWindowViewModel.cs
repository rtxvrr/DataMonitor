using DataMonitor.Data.Context;
using DataMonitor.Data.Entities;
using DataMonitor.Helpers;
using DataMonitor.Messaging;
using DataMonitor.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DataMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDbContextFactory<DataMonitorContext> _contextFactory;
        private readonly ILoggerService _loggerService;
        private readonly IWindowService _windowService;
        private readonly IFileReaderService _fileReaderService;
        private readonly IFileDialogService _fileDialogService;
        public ObservableCollection<MachineRecord> Records { get; } = new();


        private SeriesCollection _chartSeriesCollection;
        public SeriesCollection ChartSeriesCollection
        {
            get => _chartSeriesCollection;
            set { _chartSeriesCollection = value; OnPropertyChanged(); }
        }

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        private Func<double, string> _formatter;
        public Func<double, string> Formatter
        {
            get => _formatter;
            set { _formatter = value; OnPropertyChanged(); }
        }
        private string _xAxisTitle;
        public string XAxisTitle
        {
            get => _xAxisTitle;
            set { _xAxisTitle = value; OnPropertyChanged(); }
        }

        private string _yAxisTitle;
        public string YAxisTitle
        {
            get => _yAxisTitle;
            set { _yAxisTitle = value; OnPropertyChanged(); }
        }
        private GraphType _selectedGraph;
        public GraphType SelectedGraph
        {
            get => _selectedGraph;
            set
            {
                if (_selectedGraph != value)
                {
                    _selectedGraph = value;
                    OnPropertyChanged();
                    UpdateChart();
                }
            }
        }
        private string _fileContent;
        public string FileContent
        {
            get => _fileContent;
            set { _fileContent = value; OnPropertyChanged(); }
        }
        public ICommand ChangeGraphCommand { get; }
        public ICommand LogErrorCommand { get; }
        public ICommand OpenAddRecordCommand { get; }
        public ICommand ReadFileCommand { get; }
        public MainWindowViewModel (IDbContextFactory<DataMonitorContext> contextFactory, 
                                    ILoggerService loggerService, 
                                    IWindowService windowService, 
                                    IFileReaderService fileReaderService, 
                                    IFileDialogService fileDialogService)
        {
            _contextFactory = contextFactory;
            _loggerService = loggerService;
            _fileReaderService = fileReaderService;
            _fileDialogService = fileDialogService;
            _windowService = windowService;

            LogErrorCommand = new RelayCommand(async _ => await ExecuteLogErrorCommandAsync());
            ReadFileCommand = new RelayCommand(async _ => await ExecuteReadFileCommandAsync());
            OpenAddRecordCommand = new RelayCommand(_ => _windowService.ShowAddRecordWindow());

            _ = _loggerService.LogEventAsync("Приложение запущено успешно");
            Messenger.RecordAdded += OnRecordAdded;
            SelectedGraph = GraphType.MonthlyAverageGross;
            _ = LoadDataAsync();
        }
        private async void OnRecordAdded()
        {

            await LoadDataAsync();
        }
        private async Task LoadDataAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var records = await context.MachineRecord.Include(r => r.Machine).ToListAsync();
            foreach (var record in records)
                Records.Add(record);
            UpdateChart();
        }
        private void UpdateChart()
        {
            switch (SelectedGraph)
            {
                case GraphType.MonthlyAverageGross:
                    ChartSeriesCollection = CreateMonthlyAverageGrossChart(Records.ToList());
                    XAxisTitle = "Период";
                    YAxisTitle = "Вес брутто";
                    break;
                case GraphType.RecordsPerMachine:
                    ChartSeriesCollection = CreateRecordsPerMachineChart(Records.ToList());
                    XAxisTitle = "Номер машины";
                    YAxisTitle = "Количество записей";
                    break;
                case GraphType.MonthlyAverageNet:
                    ChartSeriesCollection = CreateMonthlyAverageNetChart(Records.ToList());
                    XAxisTitle = "Период";
                    YAxisTitle = "Вес нетто";
                    break;
                default:
                    break;
            }
        }
        private SeriesCollection CreateMonthlyAverageGrossChart(System.Collections.Generic.IList<MachineRecord> records)
        {
            var grouped = records.GroupBy(r => new { r.TareDate.Year, r.TareDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgGross = g.Average(r => (double)r.GrossWeight)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            Labels = grouped.Select(g => $"{g.Month}/{g.Year}").ToArray();

            var columnSeries = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "Средний вес брутто",
                Values = new ChartValues<double>(grouped.Select(g => g.AvgGross))
            };

            Formatter = value => value.ToString("F2");

            return new SeriesCollection { columnSeries };
        }

        private SeriesCollection CreateRecordsPerMachineChart(System.Collections.Generic.IList<MachineRecord> records)
        {
            var grouped = records.GroupBy(r => r.Machine.MachineNumber)
                .Select(g => new
                {
                    MachineNumber = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.MachineNumber)
                .ToList();

            Labels = grouped.Select(g => g.MachineNumber.ToString()).ToArray();

            var columnSeries = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "Количество записей",
                Values = new ChartValues<int>(grouped.Select(g => g.Count))
            };

            Formatter = value => value.ToString("N0");

            return new SeriesCollection { columnSeries };
        }
        private void CreateMonthlyAverageChart(System.Collections.Generic.IList<MachineRecord> records)
        {
            var grouped = records.GroupBy(r => new { r.TareDate.Year, r.TareDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgGross = g.Average(r => (double)r.GrossWeight)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            Labels = grouped.Select(g => $"{g.Month}/{g.Year}").ToArray();
            var values = new ChartValues<double>(grouped.Select(g => g.AvgGross));

            ChartSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Средний вес брутто",
                    Values = values
                }
            };

            Formatter = value => value.ToString("F2");
        }
        private SeriesCollection CreateMonthlyAverageNetChart(System.Collections.Generic.IList<MachineRecord> records)
        {
            var grouped = records.GroupBy(r => new { r.TareDate.Year, r.TareDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgNet = g.Average(r => (double)r.NetWeight)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            Labels = grouped.Select(g => $"{g.Month}/{g.Year}").ToArray();

            var lineSeries = new LiveCharts.Wpf.LineSeries
            {
                Title = "Средний вес нетто",
                Values = new ChartValues<double>(grouped.Select(g => g.AvgNet)),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 8
            };

            Formatter = value => value.ToString("F2");

            return new SeriesCollection { lineSeries };
        }
        private async Task ExecuteReadFileCommandAsync()
        {
            try
            {
                string filePath = _fileDialogService.OpenFileDialog("Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*");
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                FileContent = await _fileReaderService.ReadFileAsync(filePath);
                var notificationManager = new Notifications.Wpf.NotificationManager();
                notificationManager.Show(new Notifications.Wpf.NotificationContent
                {
                    Title = "Чтение файла",
                    Message = "Файл успешно прочитан",
                    Type = Notifications.Wpf.NotificationType.Success
                });
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Ошибка чтения файла: {ex.Message}");
                var notificationManager = new Notifications.Wpf.NotificationManager();
                notificationManager.Show(new Notifications.Wpf.NotificationContent
                {
                    Title = "Ошибка",
                    Message = $"Ошибка чтения файла",
                    Type = Notifications.Wpf.NotificationType.Error
                });
            }
        }
        private async Task ExecuteLogErrorCommandAsync()
        {
            try
            {
                throw new System.Exception("Вызов тестовой ошибки");
            }
            catch (System.Exception ex)
            {
                var notificationManager = new Notifications.Wpf.NotificationManager();
                notificationManager.Show(new Notifications.Wpf.NotificationContent
                {
                    Title = "Вызов ошибки",
                    Message = "Ошибка успешно вызвана и обработана",
                    Type = Notifications.Wpf.NotificationType.Success
                });
                await _loggerService.LogErrorAsync($"Ошибка вызвана: {ex.Message}");
            }
        }
        ~MainWindowViewModel()
        {
            Messenger.RecordAdded -= OnRecordAdded;
        }
    }
}
