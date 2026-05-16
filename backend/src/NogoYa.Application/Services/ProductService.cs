using AutoMapper;
using Microsoft.Extensions.Logging;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Services;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork uow, IMapper mapper, ILogger<ProductService> logger)
    { _uow = uow; _mapper = mapper; _logger = logger; }

    // Defensive cap: clients cannot request larger pages than this, regardless of input.
    private const int MaxPageSize = 25;

    public async Task<Result<PagedResult<ProductDto>>> SearchAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        // Normalize page + cap pageSize server-side.
        var safeFilter = filter with
        {
            Page = filter.Page <= 0 ? 1 : filter.Page,
            PageSize = filter.PageSize is <= 0 or > MaxPageSize ? MaxPageSize : filter.PageSize
        };

        var paged = await _uow.Products.SearchAsync(safeFilter, ct);
        var items = _mapper.Map<IReadOnlyList<ProductDto>>(paged.Items);
        return Result.Success(new PagedResult<ProductDto>
        {
            Items = items, Page = paged.Page, PageSize = paged.PageSize, TotalItems = paged.TotalItems
        });
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _uow.Products.GetWithStoreAsync(id, ct);
        return p is null
            ? Result.Failure<ProductDto>($"Producto '{id}' no encontrado.", "PRODUCT_NOT_FOUND")
            : Result.Success(_mapper.Map<ProductDto>(p));
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        var ps = await _uow.Products.GetByStoreAsync(storeId, ct);
        return Result.Success(_mapper.Map<IReadOnlyList<ProductDto>>(ps));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        if (dto.DiscountPercent is < 0 or > 100)
            return Result.Failure<ProductDto>("El descuento debe estar entre 0 y 100.", "INVALID_DISCOUNT");
        if (dto.Price < 0)
            return Result.Failure<ProductDto>("El precio no puede ser negativo.", "INVALID_PRICE");

        if (await _uow.Stores.GetByIdAsync(dto.StoreId, ct) is null)
            return Result.Failure<ProductDto>($"Comercio '{dto.StoreId}' no encontrado.", "STORE_NOT_FOUND");

        var entity = _mapper.Map<Product>(dto);
        await _uow.Products.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var fresh = await _uow.Products.GetWithStoreAsync(entity.Id, ct);
        return Result.Success(_mapper.Map<ProductDto>(fresh!));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        if (dto.DiscountPercent is < 0 or > 100)
            return Result.Failure<ProductDto>("El descuento debe estar entre 0 y 100.", "INVALID_DISCOUNT");

        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null) return Result.Failure<ProductDto>($"Producto '{id}' no encontrado.", "PRODUCT_NOT_FOUND");

        _mapper.Map(dto, product);
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        var fresh = await _uow.Products.GetWithStoreAsync(id, ct);
        return Result.Success(_mapper.Map<ProductDto>(fresh!));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null) return Result.Failure($"Producto '{id}' no encontrado.", "PRODUCT_NOT_FOUND");
        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
