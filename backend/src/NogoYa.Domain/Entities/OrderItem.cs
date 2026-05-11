using NogoYa.Domain.Common;

namespace NogoYa.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal Subtotal { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }

    public void RecalculateSubtotal()
    {
        var effectiveUnit = UnitPrice - (UnitPrice * DiscountPercent / 100m);
        Subtotal = Math.Round(effectiveUnit * Quantity, 2, MidpointRounding.ToEven);
    }
}
