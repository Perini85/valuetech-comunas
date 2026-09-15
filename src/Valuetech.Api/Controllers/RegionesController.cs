using MediatR;
using Microsoft.AspNetCore.Mvc;
using Valuetech.Application.Features.Regiones.Common;
using Valuetech.Application.Features.Regiones.ListarRegiones;
using Valuetech.Application.Features.Regiones.ObtenerRegionPorId;
using Valuetech.Application.Common.Exceptions;

namespace Valuetech.Api.Controllers;

[ApiController]
[Route("region")]
public sealed class RegionesController(ISender sender) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RegionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegionDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var region = await sender.Send(new ObtenerRegionPorIdQuery(id), cancellationToken)
            ?? throw new NotFoundException("La región no existe.");
        return Ok(region);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RegionDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<RegionDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var regiones = await sender.Send(
            new ListarRegionesQuery(),
            cancellationToken);

        return Ok(regiones);
    }
}
