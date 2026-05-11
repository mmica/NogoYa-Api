using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> b)
    {
        b.ToTable("stores");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.Latitude).HasPrecision(9, 6);
        b.Property(x => x.Longitude).HasPrecision(9, 6);
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.IsActive);

        b.HasMany(x => x.Products).WithOne(p => p.Store)
            .HasForeignKey(p => p.StoreId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Orders).WithOne(o => o.Store)
            .HasForeignKey(o => o.StoreId).OnDelete(DeleteBehavior.Restrict);
    }
}
