namespace NogoYa.Application.DTOs;

public record ProductDto(
    Guid Id, Guid StoreId, string StoreName, string Name, string? Description,
    string? ImageUrl, string? Sku, decimal Price, decimal DiscountPercent,
    decimal EffectivePrice, int Stock, bool IsAvailable);

public record CreateProductDto(
    Guid StoreId, string Name, string? Description, string? ImageUrl, string? Sku,
    decimal Price, decimal DiscountPercent, int Stock);

public record UpdateProductDto(
    string Name, string? Description, string? ImageUrl, string? Sku,
    decimal Price, decimal DiscountPercent, int Stock, bool IsAvailable);

/// <summary>
/// Filter for paginated product search.
/// IsAvailable: null = all (admin view), true/false = filter (public lists pass true).
/// PageSize is server-capped (see ProductService).
/// </summary>
public record ProductFilterDto(
    Guid? StoreId,
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? OnSale,
    bool? IsAvailable,
    int Page = 1,
    int PageSize = 25);
