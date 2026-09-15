using MediatR;
using Valuetech.Application.Features.Comunas.Common;

namespace Valuetech.Application.Features.Comunas.ObtenerComunaPorId;

public sealed record ObtenerComunaPorIdQuery(int RegionId, int Id) : IRequest<ComunaDto>;
