using AutoMapper;
using Microsoft.Extensions.Logging;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Services;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Services;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<StoreService> _logger;

    public StoreService(IUnitOfWork uow, IMapper mapper, ILogger<StoreService> logger)
    { _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<Result<IReadOnlyList<StoreDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var stores = await _uow.Stores.GetActiveAsync(ct);
        return Result.Success(_mapper.Map<IReadOnlyList<StoreDto>>(stores));
    }

    public async Task<Result<StoreDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetByIdAsync(id, ct);
        return store is null
            ? Result.Failure<StoreDto>($"Comercio '{id}' no encontrado.", "STORE_NOT_FOUND")
            : Result.Success(_mapper.Map<StoreDto>(store));
    }

    public async Task<Result<StoreDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetBySlugAsync(slug, ct);
        return store is null
            ? Result.Failure<StoreDto>($"Comercio '{slug}' no encontrado.", "STORE_NOT_FOUND")
            : Result.Success(_mapper.Map<StoreDto>(store));
    }

    public async Task<Result<StoreDto>> CreateAsync(CreateStoreDto dto, CancellationToken ct = default)
    {
        if (await _uow.Stores.SlugExistsAsync(dto.Slug, null, ct))
            return Result.Failure<StoreDto>($"El slug '{dto.Slug}' ya está en uso.", "SLUG_ALREADY_EXISTS");

        var entity = _mapper.Map<Store>(dto);
        await _uow.Stores.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Comercio creado: {StoreId} {Name}", entity.Id, entity.Name);
        return Result.Success(_mapper.Map<StoreDto>(entity));
    }

    public async Task<Result<StoreDto>> UpdateAsync(Guid id, UpdateStoreDto dto, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetByIdAsync(id, ct);
        if (store is null) return Result.Failure<StoreDto>($"Comercio '{id}' no encontrado.", "STORE_NOT_FOUND");
        _mapper.Map(dto, store);
        _uow.Stores.Update(store);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(_mapper.Map<StoreDto>(store));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetByIdAsync(id, ct);
        if (store is null) return Result.Failure($"Comercio '{id}' no encontrado.", "STORE_NOT_FOUND");
        _uow.Stores.Remove(store);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
