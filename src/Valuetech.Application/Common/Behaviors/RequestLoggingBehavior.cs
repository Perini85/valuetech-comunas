using MediatR;
using Microsoft.Extensions.Logging;

namespace Valuetech.Application.Common.Behaviors;

internal sealed class RequestLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Procesando {RequestName}", requestName);
        var response = await next();
        logger.LogInformation("Procesado {RequestName}", requestName);

        return response;
    }
}
