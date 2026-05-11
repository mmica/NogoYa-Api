using NogoYa.Application.Common;
using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrderDto>>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<Result<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default);
    Task<Result> CancelAsync(Guid id, string? reason, CancellationToken ct = default);
}
