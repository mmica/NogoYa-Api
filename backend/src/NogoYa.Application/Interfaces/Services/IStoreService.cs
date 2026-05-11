using NogoYa.Application.Common;
using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces.Services;

public interface IStoreService
{
    Task<Result<IReadOnlyList<StoreDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<StoreDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<StoreDto>> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<StoreDto>> CreateAsync(CreateStoreDto dto, CancellationToken ct = default);
    Task<Result<StoreDto>> UpdateAsync(Guid id, UpdateStoreDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
