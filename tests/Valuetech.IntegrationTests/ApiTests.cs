using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Features.Comunas.Common;
using Valuetech.Application.Features.Regiones.Common;
using Valuetech.Domain.Entities;

namespace Valuetech.IntegrationTests;

public sealed class ApiTests(SqlApiFixture fixture) : IClassFixture<SqlApiFixture>
{
    [SqlFact]
    public async Task Consultas_DevuelvenRegionesYComunasDeSuRegion()
    {
        var regiones = await fixture.Client.GetFromJsonAsync<RegionDto[]>("/region");
        Assert.Equal(3, regiones!.Length);
        Assert.Equal("Región de Valparaíso", (await fixture.Client.GetFromJsonAsync<RegionDto>("/region/1"))!.Nombre);
        var comunas = await fixture.Client.GetFromJsonAsync<ComunaDto[]>("/region/1/comuna");
        Assert.Equal(2, comunas!.Length);
        Assert.All(comunas, comuna => Assert.Equal(1, comuna.RegionId));
        Assert.Equal("Valparaíso", (await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1"))!.Nombre);
    }

    [SqlFact]
    public async Task Errores_DeIdsYRecursos_DevuelvenProblemDetails()
    {
        foreach (var (path, status) in new[]
        {
            ("/region/0", HttpStatusCode.BadRequest), ("/region/-1", HttpStatusCode.BadRequest),
            ("/region/99999", HttpStatusCode.NotFound), ("/region/99999/comuna", HttpStatusCode.NotFound),
            ("/region/0/comuna", HttpStatusCode.BadRequest), ("/region/1/comuna/0", HttpStatusCode.BadRequest),
            ("/region/2/comuna/1", HttpStatusCode.NotFound), ("/region/1/comuna/99999", HttpStatusCode.NotFound)
        })
        {
            using var response = await fixture.Client.GetAsync(path);
            Assert.Equal(status, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
            Assert.True(problem.TryGetProperty("traceId", out _));
            if (status == HttpStatusCode.BadRequest)
                Assert.True(problem.GetProperty("errors").EnumerateObject().Any());
        }
    }

    [SqlFact]
    public async Task Actualizar_GuardaXmlConFormatoSolicitado_YPermiteQuitarDatos()
    {
        var original = await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1");
        try
        {
            var update = new { Id = 1, Nombre = "  Comuna de prueba Ñ  ", InformacionAdicional = new InformacionAdicionalDto(4799.4m, 247552, 51.6m) };
            using var response = await fixture.Client.PostAsJsonAsync("/region/1/comuna", update);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var saved = await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1");
            Assert.Equal("Comuna de prueba Ñ", saved!.Nombre);
            Assert.Equal(update.InformacionAdicional, saved.InformacionAdicional);

            await using var sql = new SqlConnection(fixture.ConnectionString);
            await sql.OpenAsync();
            await using var command = new SqlCommand("SELECT CONVERT(nvarchar(max), InformacionAdicional) FROM dbo.Comunas WHERE IdComuna = 1", sql);
            var xml = XElement.Parse((string)(await command.ExecuteScalarAsync())!);
            Assert.Equal("Info", xml.Name.LocalName);
            Assert.Equal(4799.4m, (decimal)xml.Element("Superficie")!);
            Assert.Equal(247552, (int)xml.Element("Poblacion")!);
            Assert.Equal(51.6m, (decimal)xml.Element("Poblacion")!.Attribute("Densidad")!);

            using var clear = await fixture.Client.PostAsJsonAsync("/region/1/comuna", new { Id = 1, Nombre = saved.Nombre });
            Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
            Assert.Null((await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1"))!.InformacionAdicional);
        }
        finally
        {
            using var restored = await fixture.Client.PostAsJsonAsync("/region/1/comuna", original);
            restored.EnsureSuccessStatusCode();
        }
    }

    [SqlFact]
    public async Task Actualizar_NoInsertaNiModificaComunasDeOtraRegion()
    {
        foreach (var path in new[] { "/region/1/comuna", "/region/2/comuna" })
        {
            using var response = await fixture.Client.PostAsJsonAsync(path, new { Id = path.Contains("/2/") ? 1 : 99999, Nombre = "Inexistente" });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        Assert.Equal("Valparaíso", (await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1"))!.Nombre);
        Assert.Equal(2, (await fixture.Client.GetFromJsonAsync<ComunaDto[]>("/region/1/comuna"))!.Length);
    }

    [SqlFact]
    public async Task Duplicados_Devuelven409_YNoCambianLosDatos()
    {
        using var response = await fixture.Client.PostAsJsonAsync("/region/1/comuna", new { Id = 1, Nombre = "Viña del Mar" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain("UQ_Comunas", await response.Content.ReadAsStringAsync());
        Assert.Equal("Valparaíso", (await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1"))!.Nombre);
    }

    [SqlFact]
    public async Task DatosInvalidos_Devuelven400_YNoCambianLaComuna()
    {
        foreach (var body in new[]
        {
            "{}", "null", "{", "{\"id\":1,\"nombre\":null}", "{\"id\":1,\"nombre\":\"   \"}",
            "{\"id\":1,\"nombre\":\"Parcial\",\"informacionAdicional\":{\"superficie\":2}}",
            JsonSerializer.Serialize(new { Id = 1, Nombre = new string('x', 101) }),
            JsonSerializer.Serialize(new { Id = 1, Nombre = "Inválida", InformacionAdicional = new { Superficie = 0, Poblacion = -1, Densidad = -1 } })
        })
        {
            using var response = await fixture.Client.PostAsync("/region/1/comuna", new StringContent(body, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Equal("Valparaíso", (await fixture.Client.GetFromJsonAsync<ComunaDto>("/region/1/comuna/1"))!.Nombre);
    }

    [SqlFact]
    public async Task Merge_ActualizacionesConcurrentes_NoCreanFilas()
    {
        try
        {
            var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(i =>
                fixture.Client.PostAsJsonAsync("/region/1/comuna", new { Id = 1, Nombre = $"Concurrente {i}" })));
            foreach (var response in responses)
            {
                using (response) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            Assert.Equal(2, (await fixture.Client.GetFromJsonAsync<ComunaDto[]>("/region/1/comuna"))!.Length);
        }
        finally
        {
            using var restored = await fixture.Client.PostAsJsonAsync("/region/1/comuna", new { Id = 1, Nombre = "Valparaíso" });
            restored.EnsureSuccessStatusCode();
        }
    }

    [SqlFact]
    public async Task ErrorInesperado_Devuelve500SinDetallesInternos()
    {
        await using var factory = fixture.Factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRegionRepository>();
            services.AddSingleton<IRegionRepository, FailingRegiones>();
        }));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.GetAsync("/region");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("SensitiveDatabaseDetails", await response.Content.ReadAsStringAsync());
    }

    private sealed class FailingRegiones : IRegionRepository
    {
        public Task<IReadOnlyCollection<Region>> ListarAsync(CancellationToken ct) => throw new InvalidOperationException("SensitiveDatabaseDetails");
        public Task<Region?> ObtenerPorIdAsync(int id, CancellationToken ct) => throw new InvalidOperationException("SensitiveDatabaseDetails");
    }
}
