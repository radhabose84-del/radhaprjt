#nullable disable

using FluentValidation;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;

namespace SalesManagement.Presentation.Validation.BusinessUnit
{
    public class UpdateBusinessUnitCommandValidator : AbstractValidator<UpdateBusinessUnitCommand>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public UpdateBusinessUnitCommandValidator(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // ID validation
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Business Unit ID is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Business Unit not found.");

            // Business Unit Name validation
            RuleFor(x => x.BusinessUnitName)
                .NotEmpty().WithMessage("Business Unit Name is required.")
                .MaximumLength(100).WithMessage("Business Unit Name cannot exceed 100 characters.");

            // Description validation (optional)
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            // IsActive validation
            RuleFor(x => x.IsActive)
                .Must(x => x == 0 || x == 1).WithMessage("Status must be Active (1) or Inactive (0).");
        }
    }
}
