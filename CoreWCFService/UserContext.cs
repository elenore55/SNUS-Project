using System.Data.Entity;


namespace CoreWCFService
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}