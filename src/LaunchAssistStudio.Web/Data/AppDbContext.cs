using LaunchAssistStudio.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaunchAssistStudio.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadNote> LeadNotes => Set<LeadNote>();
    public DbSet<LeadStatusHistory> LeadStatusHistory => Set<LeadStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(lead =>
        {
            lead.HasIndex(x => x.PublicId).IsUnique();
            lead.HasIndex(x => x.SubmittedAtUtc);
            lead.HasIndex(x => x.Status);
            lead.HasIndex(x => x.Email);

            lead.HasMany(x => x.Notes)
                .WithOne(x => x.Lead)
                .HasForeignKey(x => x.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            lead.HasMany(x => x.StatusHistory)
                .WithOne(x => x.Lead)
                .HasForeignKey(x => x.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
