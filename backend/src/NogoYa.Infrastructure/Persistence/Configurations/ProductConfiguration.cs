using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products", tb =>
        {
            tb.HasCheckConstraint("ck_products_price_nonneg", "\"Price\" >= 0");
            tb.HasCheckConstraint("ck_products_discount_range", "\"DiscountPercent\" >= 0 AND \"DiscountPercent\" <= 100");
            tb.HasCheckConstraint("ck_products_stock_nonneg", "\"Stock\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.Property(x => x.Sku).HasMaxLength(80);
        b.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.DiscountPercent).HasPrecision(5, 2).HasDefaultValue(0m);
        b.Property(x => x.Stock).HasDefaultValue(0);
        b.Property(x => x.IsAvailable).HasDefaultValue(true);

        b.UseXminAsConcurrencyToken();

        b.HasIndex(x => x.StoreId);
        b.HasIndex(x => x.Name);
        b.HasIndex(x => new { x.StoreId, x.Sku }).IsUnique().HasFilter("\"Sku\" IS NOT NULL");
    }
}
