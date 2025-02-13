namespace DataMonitor.Data.Entities
{
    public class LogType
    {
        public int ID { get; set; }
        public string TypeTitle { get; set; }
        public ICollection<Log> Logs { get; set; }
    }
}
