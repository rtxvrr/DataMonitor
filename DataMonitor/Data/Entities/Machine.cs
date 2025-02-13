namespace DataMonitor.Data.Entities
{
    public class Machine
    {
        public int ID { get; set; }
        public int MachineNumber { get; set; }
        public ICollection<MachineRecord> MachineRecords { get; set; }
    }
}
