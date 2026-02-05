using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using FluentValidation;

namespace InventoryManagement.API.Validation.HSNMaster
{
    public class UpdateHSNMasterCommandValidator: AbstractValidator<UpdateHSNMasterCommand>
    {
        private readonly IHSNMasterQueryRepository _hsnMasterQueryRepository;

        public UpdateHSNMasterCommandValidator(IHSNMasterQueryRepository hsnMasterQueryRepository)
        {
            _hsnMasterQueryRepository = hsnMasterQueryRepository;

            // ✅ 1. Basic required field validations
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Invalid Id provided.");

            RuleFor(x => x.HSNCode)
                .NotEmpty().WithMessage("HSN Code is required.")
                .MaximumLength(10).WithMessage("HSN Code cannot exceed 10 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.");

            RuleFor(x => x.TypeId)
                .GreaterThan(0).WithMessage("Valid TypeId is required.");

            RuleFor(x => x.GSTCategoryId)
                .GreaterThan(0).WithMessage("Valid GSTCategoryId is required.");

            RuleFor(x => x.GSTPercentage)
                .GreaterThan(0).WithMessage("GST Percentage must be greater than zero.");

            RuleFor(x => x.ValidFrom)
                .NotEmpty().WithMessage("ValidFrom date is required.");

            // ✅ 2. Check if record exists
            RuleFor(x => x.Id)
                .MustAsync(async (id, cancellation) =>
                    !await _hsnMasterQueryRepository.NotFoundAsync(id))
                .WithMessage("HSN Master record not found.");

            // ✅ 3. Duplicate HSNCode excluding current record
            RuleFor(x => x)
                .MustAsync(async (command, cancellation) =>
                    !await _hsnMasterQueryRepository.AlreadyExistsAsync(command.HSNCode, command.Id))
                .WithMessage("HSN Code already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.HSNCode));
                
            // Character validation – only alphanumeric + spaces (no special chars)
            const string alphaNumWithSpacePattern = @"^[A-Za-z0-9 ]+$";

            RuleFor(x => (x.HSNCode ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.HSNCode))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");
                
        }
    }
}