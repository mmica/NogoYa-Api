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
        services.AddAutoMapper(asm);
        services.AddValidatorsFromAssembly(asm);

        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductImportService, ProductImportService>();
        return services;
    }
}
