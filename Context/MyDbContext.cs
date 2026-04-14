using Microsoft.EntityFrameworkCore;
using UC.eComm.Publish.Model;

namespace UC.eComm.Publish.Context
{
    
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Product> eco_product { get; set; }
    }
    
}
