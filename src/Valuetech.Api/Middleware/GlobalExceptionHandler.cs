using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Valuetech.Application.Common.Exceptions;

namespace Valuetech.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(validation.Errors
                .GroupBy(x => x.PropertyName).ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).Distinct().ToArray()))
                { Status = 400, Title = "Revisa los datos enviados." },
            NotFoundException => new ProblemDetails { Status = 404, Title = exception.Message },
            ConflictException => new ProblemDetails { Status = 409, Title = exception.Message },
            _ => new ProblemDetails { Status = 500, Title = "Ocurrió un error al procesar la solicitud." }
        };
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (problem.Status == 500)
            logger.LogError(exception, "Error inesperado. TraceId: {TraceId}", context.TraceIdentifier);

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
