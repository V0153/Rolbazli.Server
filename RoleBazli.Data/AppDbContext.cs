using Microsoft.EntityFrameworkCore;

namespace RoleBazli.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
