#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;

namespace SalesManagement.Presentation.Validation.SalesOrganisation
{
    public class UpdateSalesOrganisationCommandValidator : AbstractValidator<UpdateSalesOrganisationCommand>
    {
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public UpdateSalesOrganisationCommandValidator(ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.");

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
