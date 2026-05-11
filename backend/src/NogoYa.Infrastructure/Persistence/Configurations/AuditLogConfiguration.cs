using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.OldValues).HasColumnType("jsonb");
        b.Property(x => x.NewValues).HasColumnType("jsonb");
        b.Property(x => x.ChangedColumns).HasMaxLength(500);
        b.Property(x => x.UserId).HasMaxLength(100);
        b.Property(x => x.UserName).HasMaxLength(150);
        b.Property(x => x.IpAddress).HasMaxLength(60);
        b.Property(x => x.UserAgent).HasMaxLength(500);
        b.Property(x => x.Reason).HasMaxLength(500);
        b.HasIndex(x => new { x.EntityName, x.EntityId });
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.Action);
    }
}
