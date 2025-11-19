using KCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace KCRM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers { get; set; }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Notes> Notes { get; set; }

    }
}
