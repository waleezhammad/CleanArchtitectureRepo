using IntegrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationService.Infrastructure.Persistence;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Request> Requests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RequestId)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.RequestId)
                .IsUnique();

            entity.Property(e => e.ExternalRequestId)
                .HasMaxLength(100);

            entity.HasIndex(e => e.ExternalRequestId);

            entity.Property(e => e.RequestType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.RequestData)
                .IsRequired()
                .HasMaxLength(10000);

            entity.Property(e => e.ResponseData)
                .HasMaxLength(10000);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(2000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SubmittedAt);
        });
    }
}
