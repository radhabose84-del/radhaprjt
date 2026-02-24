#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;

namespace SalesManagement.Presentation.Validation.SalesGroup
{
    public class UpdateSalesGroupCommandValidator : AbstractValidator<UpdateSalesGroupCommand>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;

        public UpdateSalesGroupCommandValidator(ISalesGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.");

            // Sales Group Name — mandatory, alphanumeric + spaces
            RuleFor(x => x.SalesGroupName)
                .NotEmpty().WithMessage("Sales Group Name is required.")
                .MaximumLength(100).WithMessage("Sales Group Name cannot exceed 100 characters.")
                .Matches(@"^[A-Za-z0-9 ]+$").WithMessage("Sales Group Name must contain alphanumeric characters and spaces only.");

            // Sales Office — mandatory FK
            RuleFor(x => x.SalesOfficeId)
                .GreaterThan(0).WithMessage("Valid Sales Office is required.");

            RuleFor(x => x.SalesOfficeId)
                .MustAsync(async (id, ct) => await _queryRepository.SalesOfficeExistsAsync(id))
                .WithMessage("Sales Office does not exist or is inactive.")
                .When(x => x.SalesOfficeId > 0);

            // Uniqueness: SalesGroupName within SalesOffice (exclude self)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await _queryRepository.AlreadyExistsAsync(cmd.SalesGroupName, cmd.SalesOfficeId, cmd.Id))
                .WithMessage("Sales Group Name already exists for this Sales Office.")
                .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName) && x.SalesOfficeId > 0 && x.Id > 0);

            // Responsible Manager — optional
            RuleFor(x => x.ResponsibleManager)
                .MaximumLength(100).WithMessage("Responsible Manager cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

            // Product Category — optional, must exist if provided
            RuleFor(x => x.ProductCategoryId)
                .GreaterThan(0).WithMessage("Valid Product Category is required when provided.")
                .When(x => x.ProductCategoryId.HasValue);

            RuleFor(x => x.ProductCategoryId)
                .MustAsync(async (id, ct) => await _queryRepository.ProductCategoryExistsAsync(id.Value, ct))
                .WithMessage("Product Category does not exist in Category Master.")
                .When(x => x.ProductCategoryId.HasValue && x.ProductCategoryId.Value > 0);

            // Region / Territory — optional
            RuleFor(x => x.RegionTerritory)
                .MaximumLength(100).WithMessage("Region / Territory cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));

            // Status
            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be 0 (Inactive) or 1 (Active).");
        }
    }
}
