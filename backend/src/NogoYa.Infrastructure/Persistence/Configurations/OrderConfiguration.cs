using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
        b.Property(x => x.CustomerEmail).HasMaxLength(150).IsRequired();
        b.Property(x => x.CustomerPhone).HasMaxLength(40);
        b.Property(x => x.ShippingAddress).HasMaxLength(300);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.Total).HasPrecision(18, 2);

        b.UseXminAsConcurrencyToken();

        b.HasIndex(x => x.OrderNumber).IsUnique();
        b.HasIndex(x => x.StoreId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);

        b.HasMany(x => x.Items).WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("order_items", tb =>
        {
            tb.HasCheckConstraint("ck_order_items_qty_pos", "\"Quantity\" > 0");
            tb.HasCheckConstraint("ck_order_items_unit_nonneg", "\"UnitPrice\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.ProductName).HasMaxLength(150).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        b.Property(x => x.DiscountPercent).HasPrecision(5, 2).HasDefaultValue(0m);
        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.ProductId);
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
