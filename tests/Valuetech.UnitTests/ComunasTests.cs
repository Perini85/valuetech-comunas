using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Valuetech.Application;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Common.Exceptions;
using Valuetech.Application.Features.Comunas.ActualizarComuna;
using Valuetech.Application.Features.Comunas.Common;
using Valuetech.Application.Features.Comunas.ListarComunasPorRegion;
using Valuetech.Application.Features.Regiones.ObtenerRegionPorId;
using Valuetech.Domain.Entities;

namespace Valuetech.UnitTests;

public sealed class ComunasTests
{
    [Fact]
    public async Task Actualizar_NormalizaNombreYPreservaIdentidad()
    {
        var repository = new FakeComunas();
        using var cancellation = new CancellationTokenSource();
        var result = await new ActualizarComunaCommandHandler(repository).Handle(
            new ActualizarComunaCommand(1, 2, "  Ñuñoa  ", new(4799.4m, 247552, 51.6m)), cancellation.Token);
        Assert.Equal("Ñuñoa", result.Nombre);
        Assert.Equal(1, result.RegionId);
        Assert.Equal(2, result.Id);
        Assert.Equal(4799.4m, result.InformacionAdicional!.Superficie);
        Assert.Equal(cancellation.Token, repository.LastToken);
        Assert.Equal(1, repository.Writes);
    }

    [Fact]
    public async Task Actualizar_ComunaDeOtraRegion_NoEscribe()
    {
        var repository = new FakeComunas();
        await Assert.ThrowsAsync<NotFoundException>(() => new ActualizarComunaCommandHandler(repository)
            .Handle(new ActualizarComunaCommand(99, 2, "Cambio", null), default));
        Assert.Equal(0, repository.Writes);
    }

    [Fact]
    public async Task Actualizar_EliminadaEntreLecturaYEscritura_DevuelveNoEncontrado()
    {
        var repository = new FakeComunas { DeletedOnWrite = true };
        await Assert.ThrowsAsync<NotFoundException>(() => new ActualizarComunaCommandHandler(repository)
            .Handle(new ActualizarComunaCommand(1, 2, "Cambio", null), default));
    }

    [Fact]
    public async Task Actualizar_PermiteQuitarInformacionOpcional()
    {
        var repository = new FakeComunas();
        var result = await new ActualizarComunaCommandHandler(repository)
            .Handle(new ActualizarComunaCommand(1, 2, "Comuna", null), default);
        Assert.Null(result.InformacionAdicional);
    }

    [Theory]
    [InlineData(0, 2, "Nombre")]
    [InlineData(1, 0, "Nombre")]
    [InlineData(1, 2, "")]
    [InlineData(1, 2, "   ")]
    public void Validator_RechazaDatosInvalidos(int regionId, int id, string nombre)
        => Assert.False(new ActualizarComunaCommandValidator()
            .Validate(new ActualizarComunaCommand(regionId, id, nombre, null)).IsValid);

    [Fact]
    public void Validator_RechazaNombreLargoYNumerosFueraDeRango()
    {
        var result = new ActualizarComunaCommandValidator().Validate(
            new ActualizarComunaCommand(1, 2, new string('x', 101), new(0, -1, -1)));
        Assert.Equal(4, result.Errors.Count);
    }

    [Fact]
    public async Task Pipeline_ImpideEscrituraSiLaSolicitudEsInvalida()
    {
        var repository = new FakeComunas();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton<IComunaRepository>(repository);
        using var provider = services.BuildServiceProvider();
        await Assert.ThrowsAsync<ValidationException>(() => provider.GetRequiredService<ISender>()
            .Send(new ActualizarComunaCommand(1, 2, " ", null)));
        Assert.Equal(0, repository.Reads);
        Assert.Equal(0, repository.Writes);
    }

    [Fact]
    public async Task Listar_DistingueRegionInexistenteDeRegionVacia()
    {
        var handler = new ListarComunasPorRegionQueryHandler(new FakeRegiones(), new FakeComunas());
        Assert.Empty(await handler.Handle(new ListarComunasPorRegionQuery(1), default));
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new ListarComunasPorRegionQuery(9), default));
    }

    [Fact]
    public async Task ObtenerRegion_MapeaYDevuelveNullSiNoExiste()
    {
        var handler = new ObtenerRegionPorIdQueryHandler(new FakeRegiones());
        Assert.Equal("Región", (await handler.Handle(new(1), default))!.Nombre);
        Assert.Null(await handler.Handle(new(9), default));
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(-1, 1, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1, 1, -1)]
    public void Dominio_RechazaInformacionInvalida(decimal superficie, int poblacion, decimal densidad)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new InformacionAdicional(superficie, poblacion, densidad));

    private sealed class FakeRegiones : IRegionRepository
    {
        public Task<Region?> ObtenerPorIdAsync(int id, CancellationToken ct)
            => Task.FromResult(id == 1 ? Region.Rehidratar(1, "Región") : null);
        public Task<IReadOnlyCollection<Region>> ListarAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<Region>>([Region.Rehidratar(1, "Región")]);
    }

    private sealed class FakeComunas : IComunaRepository
    {
        public int Reads { get; private set; }
        public int Writes { get; private set; }
        public bool DeletedOnWrite { get; init; }
        public CancellationToken LastToken { get; private set; }

        public Task<Comuna?> ObtenerPorIdAsync(int regionId, int id, CancellationToken ct)
        {
            Reads++;
            return Task.FromResult(regionId == 1 && id == 2 ? Comuna.Rehidratar(2, 1, "Comuna", new(2, 10, 5)) : null);
        }
        public Task<Comuna?> ActualizarAsync(Comuna comuna, CancellationToken ct)
        {
            Writes++;
            LastToken = ct;
            return Task.FromResult(DeletedOnWrite ? null : comuna);
        }
        public Task<IReadOnlyCollection<Comuna>> ListarPorRegionAsync(int regionId, CancellationToken ct)
            => Task.FromResult<IReadOnlyCollection<Comuna>>([]);
    }
}
