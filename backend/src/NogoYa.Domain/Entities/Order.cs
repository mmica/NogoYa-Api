using NogoYa.Domain.Common;
using NogoYa.Domain.Enums;
using NogoYa.Domain.Exceptions;

namespace NogoYa.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }

    public Store? Store { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public void RecalculateTotal()
        => Total = Math.Round(Items.Sum(i => i.Subtotal), 2, MidpointRounding.ToEven);

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Cancelled || Status == OrderStatus.Refunded)
            throw new BusinessRuleException($"No se puede modificar un pedido {Status}.");
        Status = newStatus;
    }
}
