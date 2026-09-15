using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Infrastructure.Persistence.Repositories;
using Valuetech.Infrastructure.Persistence.SqlServer;

namespace Valuetech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            SqlConnectionFactory.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"No se configuró la cadena de conexión '{SqlConnectionFactory.ConnectionStringName}'.");

        services.AddSingleton(
            new SqlConnectionFactory(connectionString));

        services.AddScoped<
            IRegionRepository,
            RegionRepository>();

        return services;
    }
}