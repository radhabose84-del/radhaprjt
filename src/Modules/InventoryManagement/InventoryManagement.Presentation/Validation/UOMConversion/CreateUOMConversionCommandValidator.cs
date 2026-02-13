using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.UOMConversion
{
    public class CreateUOMConversionCommandValidator  : AbstractValidator<CreateUOMConversionCommand>
    {
        private readonly IUOMConversionQueryRepository _queryRepository;

        public CreateUOMConversionCommandValidator(IUOMConversionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.FromUOMId)
                .NotEmpty().WithMessage("From UOM is required.")
                .GreaterThan(0).WithMessage("From UOM must be a valid ID.");

            RuleFor(x => x.ToUOMId)
                .NotEmpty().WithMessage("To UOM is required.")
                .GreaterThan(0).WithMessage("To UOM must be a valid ID.")
                .NotEqual(x => x.FromUOMId).WithMessage("From and To UOM cannot be the same.");

            RuleFor(x => x.ConversionValue)
                .NotEmpty().WithMessage("Conversion value is required.")
                .GreaterThan(0).WithMessage("Conversion value must be greater than zero.")
                .PrecisionScale(18, 6, true)
                .WithMessage("Conversion value can have up to 6 decimal places.");

            // ✅ Async duplicate check
            RuleFor(x => new { x.FromUOMId, x.ToUOMId })
                .MustAsync(async (data, cancellation) =>
                {
                    return !await _queryRepository.AlreadyExistsAsync(data.FromUOMId, data.ToUOMId);
                })
                .WithMessage("A conversion for this UOM pair already exists.");
        }
    }
}