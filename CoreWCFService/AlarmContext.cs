using System.Data.Entity;

namespace CoreWCFService
{
    public class AlarmContext : DbContext
    {
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }
    }
}