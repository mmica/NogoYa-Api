using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NogoYa.Application.Interfaces.Services;
using NogoYa.Application.Services;

namespace NogoYa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var asm = Assembly.GetExecutingAssembly();
        // AutoMapper 14+ requires the configuration-delegate signature.
        services.AddAutoMapper(cfg => cfg.AddMaps(asm));
        services.AddValidatorsFromAssembly(asm);

        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductImportService, ProductImportService>();
        return services;
    }
}
