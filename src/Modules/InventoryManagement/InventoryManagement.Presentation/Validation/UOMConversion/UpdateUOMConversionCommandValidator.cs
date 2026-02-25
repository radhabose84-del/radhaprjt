using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.UOMConversion
{
    public class UpdateUOMConversionCommandValidator  : AbstractValidator<UpdateUOMConversionCommand>
    {
        private readonly IUOMConversionQueryRepository _queryRepository;

        public UpdateUOMConversionCommandValidator(IUOMConversionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // ✅ Rule: ID is required
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("UOM Conversion ID is required.");

            // ✅ Rule: FromUOMId is required
            RuleFor(x => x.FromUOMId)
                .GreaterThan(0)
                .WithMessage("From UOM is required.");

            // ✅ Rule: ToUOMId is required
            RuleFor(x => x.ToUOMId)
                .GreaterThan(0)
                .WithMessage("To UOM is required.")
                .NotEqual(x => x.FromUOMId)
                .WithMessage("From UOM and To UOM cannot be the same.");

            // ✅ Rule: Conversion value > 0
            RuleFor(x => x.ConversionValue)
                .GreaterThan(0)
                .WithMessage("Conversion value must be greater than zero.")
                .PrecisionScale(18, 6, true)
                .WithMessage("Conversion value can have up to 6 decimal places.");

            // ✅ Async duplicate check (excluding current record)
            RuleFor(x => new { x.Id, x.FromUOMId, x.ToUOMId })
                .MustAsync(async (data, cancellation) =>
                {
                    return !await _queryRepository.AlreadyExistsAsync(data.FromUOMId, data.ToUOMId, data.Id);
                })
                .WithMessage("A conversion for this UOM pair already exists.");
        }
        
    }
}