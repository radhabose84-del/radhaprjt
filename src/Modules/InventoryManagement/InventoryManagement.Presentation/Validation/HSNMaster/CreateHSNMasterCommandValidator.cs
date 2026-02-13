using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.HSNMaster
{
    public class CreateHSNMasterCommandValidator  : AbstractValidator<CreateHSNMasterCommand>
    {
        private readonly IHSNMasterQueryRepository _hsnMasterQueryRepository;

        public CreateHSNMasterCommandValidator(IHSNMasterQueryRepository hsnMasterQueryRepository)
        {
            _hsnMasterQueryRepository = hsnMasterQueryRepository;

            //  Required Fields
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

            // HSNCode must be unique
            RuleFor(x => x.HSNCode)
                .MustAsync(async (code, cancellation) =>
                    !await _hsnMasterQueryRepository.AlreadyExistsAsync(code))
                .WithMessage("HSN Code already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.HSNCode));

            // Character validation – only alphanumeric + spaces (no special chars)
            const string alphaNumWithSpacePattern = @"^[A-Za-z0-9 ]+$";

            RuleFor(x => (x.HSNCode ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.HSNCode))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");

            //  Foreign key validation for TypeId
            // RuleFor(x => x.TypeId)
            //     .MustAsync(async (typeId, cancellation) =>
            //         await _hsnMasterQueryRepository.FKColumnValidation(typeId))
            //     .WithMessage("Invalid TypeId: record does not exist in MiscMaster.")
            //     .When(x => x.TypeId > 0);

            // //  Foreign key validation for GSTCategoryId
            // RuleFor(x => x.GSTCategoryId)
            //     .MustAsync(async (gstCatId, cancellation) =>
            //         await _hsnMasterQueryRepository.FKColumnValidation(gstCatId))
            //     .WithMessage("Invalid GSTCategoryId: record does not exist in MiscMaster.")
            //     .When(x => x.GSTCategoryId > 0);
        }
        
    }
}