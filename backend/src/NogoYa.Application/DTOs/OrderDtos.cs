using NogoYa.Domain.Enums;

namespace NogoYa.Application.DTOs;

public record OrderItemDto(
    Guid Id, Guid ProductId, string ProductName, int Quantity,
    decimal UnitPrice, decimal DiscountPercent, decimal Subtotal);

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record OrderDto(
    Guid Id, string OrderNumber, Guid StoreId, string StoreName,
    string CustomerName, string CustomerEmail, string? CustomerPhone,
    string? ShippingAddress, string? Notes, OrderStatus Status, decimal Total,
    DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items);

public record CreateOrderDto(
    Guid StoreId, string CustomerName, string CustomerEmail,
    string? CustomerPhone, string? ShippingAddress, string? Notes,
    IReadOnlyList<CreateOrderItemDto> Items);

public record UpdateOrderStatusDto(OrderStatus NewStatus, string? Reason);
