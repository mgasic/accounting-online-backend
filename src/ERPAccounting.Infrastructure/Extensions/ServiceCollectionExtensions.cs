using ERPAccounting.Domain.Abstractions.Gateways;
using ERPAccounting.Infrastructure.Data;
using ERPAccounting.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERPAccounting.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // Repositories & Gateways
        services.AddScoped<IStoredProcedureGateway, StoredProcedureGateway>();


        return services;
    }
}
