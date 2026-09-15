using FluentValidation;

namespace Valuetech.Application.Features.Comunas.ActualizarComuna;

public sealed class ActualizarComunaCommandValidator : AbstractValidator<ActualizarComunaCommand>
{
    public ActualizarComunaCommandValidator()
    {
        RuleFor(x => x.RegionId).GreaterThan(0).WithMessage("El identificador de la región debe ser mayor que cero.");
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El identificador de la comuna debe ser mayor que cero.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        When(x => x.InformacionAdicional is not null, () =>
        {
            RuleFor(x => x.InformacionAdicional!.Superficie).GreaterThan(0)
                .WithMessage("La superficie debe ser mayor que cero.");
            RuleFor(x => x.InformacionAdicional!.Poblacion).GreaterThanOrEqualTo(0)
                .WithMessage("La población no puede ser negativa.");
            RuleFor(x => x.InformacionAdicional!.Densidad).GreaterThanOrEqualTo(0)
                .WithMessage("La densidad no puede ser negativa.");
        });
    }
}
