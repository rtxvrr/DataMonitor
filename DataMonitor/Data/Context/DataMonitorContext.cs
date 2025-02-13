using DataMonitor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataMonitor.Data.Context
{
    public class DataMonitorContext : DbContext
    {
        public DbSet<Log> Log { get; set; }
        public DbSet<LogType> LogType { get; set; }
        public DbSet<Machine> Machine { get; set; }
        public DbSet<MachineRecord> MachineRecord { get; set; }

        public DataMonitorContext(DbContextOptions<DataMonitorContext> options)
            : base(options)
        {

        }
    }
}
