using MediatR;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Common.Exceptions;
using Valuetech.Application.Features.Comunas.Common;

namespace Valuetech.Application.Features.Comunas.ObtenerComunaPorId;

public sealed class ObtenerComunaPorIdQueryHandler(IComunaRepository comunas)
    : IRequestHandler<ObtenerComunaPorIdQuery, ComunaDto>
{
    public async Task<ComunaDto> Handle(ObtenerComunaPorIdQuery request, CancellationToken cancellationToken)
    {
        var comuna = await comunas.ObtenerPorIdAsync(request.RegionId, request.Id, cancellationToken)
            ?? throw new NotFoundException("La comuna no existe en la región indicada.");
        return ComunaDto.FromEntity(comuna);
    }
}
