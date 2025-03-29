using Freddie.Helpers.Services;
using Freddie.Models;
using Microsoft.EntityFrameworkCore;

namespace Freddie.Helpers.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateEvaluation> Evaluations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candidate>()
                .Property(c => c.AvailableImmediately)
                .HasDefaultValue(false);

            modelBuilder.Entity<Candidate>()
                .Property(c => c.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Candidate>()
                .Property(c => c.ModifiedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // One-to-One Relationship: Candidate ↔ CandidateEvaluation
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.AIEvaluation)
                .WithOne()
                .HasForeignKey<CandidateEvaluation>(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade); // Delete evaluation if candidate is deleted
        }

    }
}
