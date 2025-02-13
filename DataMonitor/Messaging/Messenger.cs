namespace DataMonitor.Messaging
{
    public static class Messenger
    {
        public static event Action RecordAdded;
        public static void NotifyRecordAdded() => RecordAdded?.Invoke();
    }
}
