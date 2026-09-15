using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using Valuetech.WebMvc.Models;
using Valuetech.WebMvc.Services;

namespace Valuetech.UnitTests;

public sealed class WebMvcTests
{
    [Fact]
    public void Formulario_PermiteDatosOpcionalesVacios_PeroNoParciales()
    {
        var model = new EditarComunaModel { Id = 1, RegionId = 1, Nombre = "Comuna" };
        var errors = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(model, new ValidationContext(model), errors, true));
        model.Superficie = 5;
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), errors, true));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(model.Poblacion)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(model.Densidad)));
    }

    [Fact]
    public async Task Cliente_EnviaContratoJsonALaRutaDeLaRegion()
    {
        var handler = new StubHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/region/3/comuna", request.RequestUri!.AbsolutePath);
            var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal(2, json.RootElement.GetProperty("id").GetInt32());
            Assert.Equal(4799.4m, json.RootElement.GetProperty("informacionAdicional").GetProperty("superficie").GetDecimal());
            Assert.False(json.RootElement.TryGetProperty("regionNombre", out _));
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") };
        await new ValuetechApiClient(http).ActualizarComuna(new EditarComunaModel
        {
            Id = 2, RegionId = 3, Nombre = "Comuna", Superficie = 4799.4m, Poblacion = 247552, Densidad = 51.6m
        }, default);
    }

    [Fact]
    public async Task Cliente_ConservaErroresDeValidacionDeLaApi()
    {
        using var http = new HttpClient(new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"title\":\"Revisa los datos.\",\"errors\":{\"Nombre\":[\"Obligatorio\"]}}", Encoding.UTF8, "application/problem+json")
        }))) { BaseAddress = new Uri("https://localhost/") };
        var ex = await Assert.ThrowsAsync<ApiException>(() => new ValuetechApiClient(http).ListarRegiones(default));
        Assert.Equal("Obligatorio", ex.Errors["Nombre"][0]);
    }

    [Fact]
    public async Task Cliente_ManejaErrorHtmlSinExponerlo()
    {
        using var http = new HttpClient(new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html>proxy internal error</html>", Encoding.UTF8, "text/html")
        }))) { BaseAddress = new Uri("https://localhost/") };
        var ex = await Assert.ThrowsAsync<ApiException>(() => new ValuetechApiClient(http).ListarRegiones(default));
        Assert.DoesNotContain("proxy", ex.Message);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handle) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handle(request);
    }
}
