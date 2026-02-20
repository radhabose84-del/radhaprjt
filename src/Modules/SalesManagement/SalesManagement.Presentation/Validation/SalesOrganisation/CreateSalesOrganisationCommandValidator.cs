#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;

namespace SalesManagement.Presentation.Validation.SalesOrganisation
{
    public class CreateSalesOrganisationCommandValidator : AbstractValidator<CreateSalesOrganisationCommand>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public CreateSalesOrganisationCommandValidator(ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.SalesOrganisationCode)
                .NotEmpty().WithMessage("Sales Organisation Code is required.")
                .MaximumLength(20).WithMessage("Sales Organisation Code cannot exceed 20 characters.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("Sales Organisation Code must be alphanumeric only.");

            RuleFor(x => x.SalesOrganisationCode)
                .MustAsync(async (code, cancellation) =>
                    !await _queryRepository.AlreadyExistsAsync(code))
                .WithMessage("Sales Organisation Code already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.SalesOrganisationCode));

            RuleFor(x => x.SalesOrganisationName)
                .NotEmpty().WithMessage("Sales Organisation Name is required.")
                .MaximumLength(100).WithMessage("Sales Organisation Name cannot exceed 100 characters.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Valid CompanyId is required.");

            RuleFor(x => x.CompanyId)
                .MustAsync(async (companyId, cancellation) =>
                    await _queryRepository.CompanyExistsAsync(companyId))
                .WithMessage("CompanyId does not exist in Company Master.")
                .When(x => x.CompanyId > 0);
        }
    }
}
