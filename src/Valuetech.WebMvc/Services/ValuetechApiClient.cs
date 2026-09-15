using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Valuetech.WebMvc.Models;

namespace Valuetech.WebMvc.Services;

public sealed class ApiException(HttpStatusCode statusCode, string message,
    IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();
}

public sealed class ValuetechApiClient(HttpClient httpClient)
{
    public Task<List<RegionModel>> ListarRegiones(CancellationToken ct)
        => Get<List<RegionModel>>("region", ct);
    public Task<RegionModel> ObtenerRegion(int id, CancellationToken ct)
        => Get<RegionModel>($"region/{id}", ct);
    public Task<List<ComunaModel>> ListarComunas(int regionId, CancellationToken ct)
        => Get<List<ComunaModel>>($"region/{regionId}/comuna", ct);
    public Task<ComunaModel> ObtenerComuna(int regionId, int id, CancellationToken ct)
        => Get<ComunaModel>($"region/{regionId}/comuna/{id}", ct);

    public async Task ActualizarComuna(EditarComunaModel model, CancellationToken ct)
    {
        using var response = await httpClient.PostAsJsonAsync($"region/{model.RegionId}/comuna", new
        {
            model.Id, model.Nombre,
            InformacionAdicional = model.Superficie.HasValue
                ? new InformacionAdicionalModel(model.Superficie.Value, model.Poblacion!.Value, model.Densidad!.Value)
                : null
        }, ct);
        await EnsureSuccess(response, ct);
    }

    private async Task<T> Get<T>(string path, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync(path, ct);
        await EnsureSuccess(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new HttpRequestException("El servicio entregó una respuesta vacía.");
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        ValidationProblemDetails? problem = null;
        try { problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct); }
        catch (JsonException) { /* Un proxy puede devolver HTML en lugar de ProblemDetails. */ }
        throw new ApiException(response.StatusCode,
            (int)response.StatusCode >= 500 ? "El servicio no está disponible. Intenta nuevamente."
                : problem?.Title ?? "No fue posible completar la solicitud.", problem?.Errors);
    }
}
