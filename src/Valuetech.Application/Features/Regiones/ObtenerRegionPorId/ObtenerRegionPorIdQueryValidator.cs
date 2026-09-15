using FluentValidation;

namespace Valuetech.Application.Features.Regiones.ObtenerRegionPorId;

public sealed class ObtenerRegionPorIdQueryValidator : AbstractValidator<ObtenerRegionPorIdQuery>
{
    public ObtenerRegionPorIdQueryValidator()
        => RuleFor(x => x.Id).GreaterThan(0).WithMessage("El identificador de la región debe ser mayor que cero.");
}
