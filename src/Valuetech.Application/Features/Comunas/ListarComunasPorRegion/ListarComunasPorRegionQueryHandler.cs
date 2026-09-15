using MediatR;
using Valuetech.Application.Abstractions.Persistence;
using Valuetech.Application.Common.Exceptions;
using Valuetech.Application.Features.Comunas.Common;

namespace Valuetech.Application.Features.Comunas.ListarComunasPorRegion;

public sealed class ListarComunasPorRegionQueryHandler(IRegionRepository regiones, IComunaRepository comunas)
    : IRequestHandler<ListarComunasPorRegionQuery, IReadOnlyCollection<ComunaDto>>
{
    public async Task<IReadOnlyCollection<ComunaDto>> Handle(ListarComunasPorRegionQuery request, CancellationToken cancellationToken)
    {
        if (await regiones.ObtenerPorIdAsync(request.RegionId, cancellationToken) is null)
            throw new NotFoundException("La región no existe.");

        var result = await comunas.ListarPorRegionAsync(request.RegionId, cancellationToken);
        return result.Select(ComunaDto.FromEntity).ToArray();
    }
}
