#nullable disable

using FluentValidation;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;

namespace SalesManagement.Presentation.Validation.BusinessUnit
{
    public class CreateBusinessUnitCommandValidator : AbstractValidator<CreateBusinessUnitCommand>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public CreateBusinessUnitCommandValidator(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // Business Unit Code validation
            RuleFor(x => x.BusinessUnitCode)
                .NotEmpty().WithMessage("Business Unit Code is required.")
                .MaximumLength(20).WithMessage("Business Unit Code cannot exceed 20 characters.")
                .Matches("^[A-Za-z0-9]+$").WithMessage("Business Unit Code must be alphanumeric.");

            RuleFor(x => x.BusinessUnitCode)
                .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                .WithMessage("Business Unit Code already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessUnitCode));

            // Business Unit Name validation
            RuleFor(x => x.BusinessUnitName)
                .NotEmpty().WithMessage("Business Unit Name is required.")
                .MaximumLength(100).WithMessage("Business Unit Name cannot exceed 100 characters.");

            // Description validation (optional)
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
