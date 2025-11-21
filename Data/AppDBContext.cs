using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Models;

namespace ProgPOEP1.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimApproval> ClaimApprovals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Lecturer entity
            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.HasKey(e => e.LecturerID);
                entity.Property(e => e.LecturerID).HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
            });

            // Configure Claim entity - FIXED VERSION
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.ClaimID);
                entity.Property(e => e.ClaimID).HasMaxLength(50);
                entity.Property(e => e.ContractorID).HasMaxLength(50); // REMOVED .IsRequired()
                entity.Property(e => e.Month).IsRequired().HasMaxLength(20); // Keep this required (from form)
                entity.Property(e => e.HoursWorked).HasColumnType("decimal(18,2)");
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(20); // REMOVED .IsRequired()
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.DocumentPath).HasMaxLength(500); // Add this if missing

                // Relationship
                entity.HasOne(e => e.Lecturer)
                      .WithMany(l => l.Claims)
                      .HasForeignKey(e => e.ContractorID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ClaimApproval entity
            modelBuilder.Entity<ClaimApproval>(entity =>
            {
                entity.HasKey(e => e.ApprovalID);
                entity.Property(e => e.ClaimID).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ApprovedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Comments).HasMaxLength(500);

                // Relationship
                entity.HasOne(e => e.Claim)
                      .WithMany()
                      .HasForeignKey(e => e.ClaimID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}