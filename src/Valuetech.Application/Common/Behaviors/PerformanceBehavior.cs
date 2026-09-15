using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Valuetech.Application.Common.Behaviors;

internal sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds >= SlowRequestThresholdMilliseconds)
        {
            logger.LogWarning(
                "La solicitud {RequestName} tardó {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
