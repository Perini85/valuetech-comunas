using FluentValidation;

namespace Valuetech.Application.Features.Comunas.ListarComunasPorRegion;

public sealed class ListarComunasPorRegionQueryValidator : AbstractValidator<ListarComunasPorRegionQuery>
{
    public ListarComunasPorRegionQueryValidator()
        => RuleFor(x => x.RegionId).GreaterThan(0).WithMessage("El identificador de la región debe ser mayor que cero.");
}
