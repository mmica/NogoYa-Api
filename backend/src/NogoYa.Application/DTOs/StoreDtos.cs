namespace NogoYa.Application.DTOs;

public record StoreDto(
    Guid Id, string Name, string Slug, string? Description, string? LogoUrl,
    string? Address, string? Phone, string? Email, bool IsActive, int ProductsCount);

public record CreateStoreDto(
    string Name, string Slug, string? Description, string? LogoUrl,
    string? Address, string? Phone, string? Email);

public record UpdateStoreDto(
    string Name, string? Description, string? LogoUrl,
    string? Address, string? Phone, string? Email, bool IsActive);
