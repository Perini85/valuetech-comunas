using MediatR;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Common.Exceptions;
using Valuetech.Application.Features.Comunas.Common;
using Valuetech.Domain.Entities;

namespace Valuetech.Application.Features.Comunas.ActualizarComuna;

public sealed class ActualizarComunaCommandHandler(IComunaRepository comunas)
    : IRequestHandler<ActualizarComunaCommand, ComunaDto>
{
    public async Task<ComunaDto> Handle(ActualizarComunaCommand request, CancellationToken cancellationToken)
    {
        var comuna = await comunas.ObtenerPorIdAsync(request.RegionId, request.Id, cancellationToken)
            ?? throw new NotFoundException("La comuna no existe en la región indicada.");
        comuna.Renombrar(request.Nombre);
        comuna.ActualizarInformacion(request.InformacionAdicional is { } info
            ? new InformacionAdicional(info.Superficie, info.Poblacion, info.Densidad)
            : null);
        var actualizada = await comunas.ActualizarAsync(comuna, cancellationToken)
            ?? throw new NotFoundException("La comuna ya no existe en la región indicada.");
        return ComunaDto.FromEntity(actualizada);
    }
}
