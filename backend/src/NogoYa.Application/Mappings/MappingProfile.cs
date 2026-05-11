using AutoMapper;
using NogoYa.Application.DTOs;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Store, StoreDto>()
            .ForCtorParam(nameof(StoreDto.ProductsCount),
                opt => opt.MapFrom(s => s.Products.Count(p => !p.IsDeleted)));
        CreateMap<CreateStoreDto, Store>();
        CreateMap<UpdateStoreDto, Store>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Slug, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore());

        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.StoreName), opt => opt.MapFrom(p => p.Store!.Name))
            .ForCtorParam(nameof(ProductDto.EffectivePrice), opt => opt.MapFrom(p => p.GetEffectivePrice()));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        CreateMap<Order, OrderDto>()
            .ForCtorParam(nameof(OrderDto.StoreName), opt => opt.MapFrom(o => o.Store!.Name))
            .ForCtorParam(nameof(OrderDto.Items), opt => opt.MapFrom(o => o.Items));
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<AuditLog, AuditLogDto>();
    }
}
