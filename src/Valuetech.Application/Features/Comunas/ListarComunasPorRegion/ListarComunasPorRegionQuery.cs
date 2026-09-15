using MediatR;
using Valuetech.Application.Features.Comunas.Common;

namespace Valuetech.Application.Features.Comunas.ListarComunasPorRegion;

public sealed record ListarComunasPorRegionQuery(int RegionId) : IRequest<IReadOnlyCollection<ComunaDto>>;
