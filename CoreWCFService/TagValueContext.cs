using System.Data.Entity;


namespace CoreWCFService
{
    public class TagValueContext : DbContext
    {
        public DbSet<TagValue> TagValues { get; set; }
    }
}