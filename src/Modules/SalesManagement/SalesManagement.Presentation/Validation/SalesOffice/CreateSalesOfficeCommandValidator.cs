using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;

namespace SalesManagement.Presentation.Validation.SalesOffice
{
    public class CreateSalesOfficeCommandValidator : AbstractValidator<CreateSalesOfficeCommand>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;

        public CreateSalesOfficeCommandValidator(ISalesOfficeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // Sales Office Name
            RuleFor(x => x.SalesOfficeName)
                .NotEmpty().WithMessage("Sales Office Name is required.")
                .MaximumLength(100).WithMessage("Sales Office Name cannot exceed 100 characters.")
                .Matches(@"^[A-Za-z0-9 ]+$").WithMessage("Sales Office Name must contain alphanumeric characters and spaces only.");

            // Sales Organisation FK
            RuleFor(x => x.SalesOrganisationId)
                .GreaterThan(0).WithMessage("Valid Sales Organisation is required.");

            RuleFor(x => x.SalesOrganisationId)
                .MustAsync(async (id, ct) => await _queryRepository.SalesOrganisationExistsAsync(id))
                .WithMessage("Sales Organisation does not exist or is inactive.")
                .When(x => x.SalesOrganisationId > 0);

            // Uniqueness: SalesOfficeName within SalesOrganisation (BR-2)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await _queryRepository.AlreadyExistsAsync(cmd.SalesOfficeName, cmd.SalesOrganisationId))
                .WithMessage("Sales Office Name already exists for this Sales Organisation.")
                .When(x => !string.IsNullOrWhiteSpace(x.SalesOfficeName) && x.SalesOrganisationId > 0);

            // City FK
            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("Valid City is required.");

            RuleFor(x => x.CityId)
                .MustAsync(async (id, ct) => await _queryRepository.CityExistsAsync(id))
                .WithMessage("City does not exist in City Master.")
                .When(x => x.CityId > 0);

            // Pincode - numeric only
            RuleFor(x => x.Pincode)
                .Matches(@"^\d+$").WithMessage("Pincode must contain numeric values only.")
                .MaximumLength(20).WithMessage("Pincode cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Pincode));

            // Phone
            RuleFor(x => x.Phone)
                .Matches(@"^[+\d\s\-\(\)]+$").WithMessage("Phone must be a valid phone number format.")
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            // Email
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            // Responsible Manager
            RuleFor(x => x.ResponsibleManager)
                .MaximumLength(100).WithMessage("Responsible Manager cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

            // Region / Territory
            RuleFor(x => x.RegionTerritory)
                .MaximumLength(100).WithMessage("Region / Territory cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));

            // Address
            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
        }
    }
}
