
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrganisation
{
    public class UpdateSalesOrganisationCommandValidator : AbstractValidator<UpdateSalesOrganisationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public UpdateSalesOrganisationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesOrganisation>("SalesOrganisationName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesOrganisation>("Description") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.SalesOrganisationName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error}");

                        RuleFor(x => x.CompanyId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.CompanyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.CompanyId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesOrganisationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Organisation {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CompanyId)
                            .MustAsync(async (companyId, ct) =>
                                await _queryRepository.CompanyExistsAsync(companyId))
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.CompanyId)} {rule.Error}")
                            .When(x => x.CompanyId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesOrganisationCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
