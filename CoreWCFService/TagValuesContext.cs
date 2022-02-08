using System.Data.Entity;


namespace CoreWCFService
{
    public class TagValuesContext : DbContext
    {
        public DbSet<TagValue> TagValues { get; set; }
    }
}