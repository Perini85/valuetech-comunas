using FluentValidation;

namespace Valuetech.Application.Features.Comunas.ObtenerComunaPorId;

public sealed class ObtenerComunaPorIdQueryValidator : AbstractValidator<ObtenerComunaPorIdQuery>
{
    public ObtenerComunaPorIdQueryValidator()
    {
        RuleFor(x => x.RegionId).GreaterThan(0).WithMessage("El identificador de la región debe ser mayor que cero.");
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El identificador de la comuna debe ser mayor que cero.");
    }
}
