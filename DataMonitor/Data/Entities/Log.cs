namespace DataMonitor.Data.Entities
{
    public class Log
    {
        public int ID { get; set; }
        public string Message { get; set; }
        public int LogTypeID { get; set; }
        public DateTime CreatedAt { get; set; }
        public LogType LogType { get; set; }
    }
}
