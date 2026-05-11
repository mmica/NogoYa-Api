using NogoYa.Domain.Common;
using NogoYa.Domain.Exceptions;

namespace NogoYa.Domain.Entities;

public class Product : BaseEntity
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Store? Store { get; set; }

    public decimal GetEffectivePrice()
        => Math.Round(Price - (Price * DiscountPercent / 100m), 2, MidpointRounding.ToEven);

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new BusinessRuleException("La cantidad debe ser mayor que cero.");
        if (quantity > Stock) throw new BusinessRuleException($"Stock insuficiente para el producto {Name}.");
        Stock -= quantity;
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0) return;
        Stock += quantity;
    }
}
