using AutoMapper;
using Microsoft.Extensions.Logging;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Services;
using NogoYa.Domain.Entities;
using NogoYa.Domain.Enums;
using NogoYa.Domain.Exceptions;

namespace NogoYa.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly ICurrentUserService _currentUser;

    public OrderService(IUnitOfWork uow, IMapper mapper, ILogger<OrderService> logger, ICurrentUserService currentUser)
    { _uow = uow; _mapper = mapper; _logger = logger; _currentUser = currentUser; }

    public async Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return Result.Failure<OrderDto>("El pedido debe tener al menos un producto.", "EMPTY_ORDER");

        if (await _uow.Stores.GetByIdAsync(dto.StoreId, ct) is null)
            return Result.Failure<OrderDto>($"Comercio '{dto.StoreId}' no encontrado.", "STORE_NOT_FOUND");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var order = new Order
            {
                OrderNumber = await _uow.Orders.GenerateOrderNumberAsync(ct),
                StoreId = dto.StoreId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                ShippingAddress = dto.ShippingAddress,
                Notes = dto.Notes,
                Status = OrderStatus.Pending
            };

            foreach (var line in dto.Items)
            {
                var product = await _uow.Products.GetByIdAsync(line.ProductId, ct);
                if (product is null) throw new NotFoundException(nameof(Product), line.ProductId);
                if (!product.IsAvailable) throw new BusinessRuleException($"El producto '{product.Name}' no está disponible.");
                if (product.StoreId != dto.StoreId) throw new BusinessRuleException($"El producto '{product.Name}' no pertenece al comercio.");

                product.DecreaseStock(line.Quantity);

                var item = new OrderItem
                {
                    ProductId = product.Id, ProductName = product.Name, Quantity = line.Quantity,
                    UnitPrice = product.Price, DiscountPercent = product.DiscountPercent
                };
                item.RecalculateSubtotal();
                order.Items.Add(item);
            }

            order.RecalculateTotal();
            await _uow.Orders.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Pedido {Order} creado por {Customer}. Total: {Total}",
                order.OrderNumber, order.CustomerEmail, order.Total);

            var saved = await _uow.Orders.GetWithItemsAsync(order.Id, ct);
            return Result.Success(_mapper.Map<OrderDto>(saved!));
        }
        catch (DomainException ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            return Result.Failure<OrderDto>(ex.Message, "ORDER_CREATION_FAILED");
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var o = await _uow.Orders.GetWithItemsAsync(id, ct);
        return o is null
            ? Result.Failure<OrderDto>($"Pedido '{id}' no encontrado.", "ORDER_NOT_FOUND")
            : Result.Success(_mapper.Map<OrderDto>(o));
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        var orders = await _uow.Orders.GetByStoreAsync(storeId, ct);
        return Result.Success(_mapper.Map<IReadOnlyList<OrderDto>>(orders));
    }

    public async Task<Result<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetWithItemsAsync(id, ct);
        if (order is null) return Result.Failure<OrderDto>($"Pedido '{id}' no encontrado.", "ORDER_NOT_FOUND");

        try { order.ChangeStatus(dto.NewStatus); }
        catch (BusinessRuleException ex) { return Result.Failure<OrderDto>(ex.Message, "INVALID_STATUS_TRANSITION"); }

        _uow.Orders.Update(order);
        await _uow.AuditLogs.AddAsync(new AuditLog
        {
            EntityName = nameof(Order), EntityId = order.Id, Action = AuditAction.StatusChange,
            NewValues = $"{{\"status\":\"{dto.NewStatus}\"}}", Reason = dto.Reason,
            UserId = _currentUser.UserId, UserName = _currentUser.UserName,
            IpAddress = _currentUser.IpAddress, UserAgent = _currentUser.UserAgent
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result> CancelAsync(Guid id, string? reason, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetWithItemsAsync(id, ct);
        if (order is null) return Result.Failure($"Pedido '{id}' no encontrado.", "ORDER_NOT_FOUND");
        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded)
            return Result.Failure($"El pedido ya está {order.Status}.", "INVALID_STATUS_TRANSITION");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var line in order.Items)
            {
                var product = await _uow.Products.GetByIdAsync(line.ProductId, ct);
                product?.RestoreStock(line.Quantity);
            }
            order.Status = OrderStatus.Cancelled;
            _uow.Orders.Update(order);
            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                EntityName = nameof(Order), EntityId = order.Id, Action = AuditAction.StatusChange,
                NewValues = "{\"status\":\"Cancelled\"}", Reason = reason,
                UserId = _currentUser.UserId, UserName = _currentUser.UserName
            }, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return Result.Success();
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
