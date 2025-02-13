namespace DataMonitor.Data.Entities
{
    public class MachineRecord
    {
        public int ID { get; set; }
        public int MachineID { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public DateTime TareDate { get; set; }
        public DateTime GrossDate { get; set; }
        public Machine Machine { get; set; }
    }
}
