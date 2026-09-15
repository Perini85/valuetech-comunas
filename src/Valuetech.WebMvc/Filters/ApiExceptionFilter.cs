using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Valuetech.WebMvc.Services;

namespace Valuetech.WebMvc.Filters;

public sealed class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (ApiException or HttpRequestException or TaskCanceledException)) return;
        var status = context.Exception is ApiException api ? (int)api.StatusCode : 503;
        logger.LogWarning(context.Exception, "No se pudo completar la operación con la API.");
        context.Result = new ViewResult
        {
            ViewName = "ApiError", StatusCode = status,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
            {
                ["Message"] = context.Exception is ApiException known ? known.Message
                    : "No se pudo conectar con el servicio. Intenta nuevamente.",
                ["StatusCode"] = status
            }
        };
        context.ExceptionHandled = true;
    }
}
