using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Models;

namespace ProgPOEP1.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Contractor)
                .WithMany()
                .HasForeignKey(c => c.ContractorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lecturer>()
                .HasKey(l => l.LecturerID);

            modelBuilder.Entity<Claim>()
                .HasKey(c => c.ClaimID);

            modelBuilder.Entity<Lecturer>()
                .Property(l => l.LecturerID)
                .IsRequired();

            modelBuilder.Entity<Claim>()
                .Property(c => c.ClaimID)
                .IsRequired();

            modelBuilder.Entity<Claim>()
                .Property(c => c.ContractorID)
                .IsRequired();
        }
    }
}
