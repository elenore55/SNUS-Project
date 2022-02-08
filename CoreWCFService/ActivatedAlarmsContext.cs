using System.Data.Entity;

namespace CoreWCFService
{
    public class ActivatedAlarmsContext : DbContext
    {
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }
    }
}