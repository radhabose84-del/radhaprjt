using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.UOMConversion
{
    public class DeleteUOMConversionCommandValidator : AbstractValidator<DeleteUOMConversionCommand>
    {
        private readonly IUOMConversionQueryRepository _iUOMConversionQueryRepository;

        public DeleteUOMConversionCommandValidator(IUOMConversionQueryRepository queryRepository)
        {
            _iUOMConversionQueryRepository = queryRepository;

              RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("UOM Conversion ID is required.")
                .MustAsync(async (id, cancellation) =>
                {
                    var existing = await _iUOMConversionQueryRepository.GetByIdAsync(id);
                    return existing != null;
                })
                .WithMessage("UOM Conversion record not found.");
        }
    }
}