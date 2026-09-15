using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Valuetech.Application.Common.Behaviors;

namespace Valuetech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        return services;
    }
}
