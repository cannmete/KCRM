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
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Deal> Deals { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).HasColumnType("varchar(50)").IsRequired();
                entity.Property(e => e.Email).HasColumnType("varchar(100)");
                entity.Property(e => e.PasswordHash).HasColumnType("longblob").IsRequired();
                entity.Property(e => e.PasswordSalt).HasColumnType("longblob").IsRequired();
                entity.Property(e => e.Role).HasColumnType("varchar(20)").HasDefaultValue("User");
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.FullName).HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Email).HasColumnType("varchar(100)");
                entity.Property(e => e.Phone).HasColumnType("varchar(20)");
                entity.Property(e => e.Address).HasColumnType("varchar(200)");
                entity.Property(e => e.IsDeleted).HasColumnType("int").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            // TaskItem
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.Property(e => e.Title).HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Description).HasColumnType("varchar(300)");
                entity.Property(e => e.IsDeleted).HasColumnType("int").HasDefaultValue(0);
                entity.Property(e => e.DueDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            // Notes
            modelBuilder.Entity<Notes>(entity =>
            {
                entity.Property(e => e.Content).HasColumnType("varchar(500)").IsRequired();
                entity.Property(e => e.UserId).HasColumnType("int").IsRequired();
                entity.Property(e => e.IsDeleted).HasColumnType("int").HasDefaultValue(0);
            });

            // Deal
            modelBuilder.Entity<Deal>(entity =>
            {
                entity.Property(e => e.Title).HasColumnType("varchar(150)").IsRequired();
                entity.Property(e => e.Description).HasColumnType("varchar(500)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.IsDeleted).HasColumnType("int").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                // İlişki: Bir fırsat silinirse müşteri silinmesin (Restrict)
                entity.HasOne(d => d.Customer)
                      .WithMany(c => c.Deals)
                      .HasForeignKey(d => d.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
