#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrganisation
{
    public class CreateSalesOrganisationCommandValidator : AbstractValidator<CreateSalesOrganisationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrganisationQueryRepository _queryRepository;

        public CreateSalesOrganisationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrganisationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesOrganisation>("SalesOrganisationCode") ?? 20;
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
                        RuleFor(x => x.SalesOrganisationCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationCode)} {rule.Error}");

                        RuleFor(x => x.SalesOrganisationName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error}");

                        RuleFor(x => x.CompanyId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.CompanyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.CompanyId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesOrganisationCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.SalesOrganisationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesOrganisationCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOrganisationCode));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CompanyId)
                            .MustAsync(async (companyId, ct) =>
                                await _queryRepository.CompanyExistsAsync(companyId))
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.CompanyId)} {rule.Error}")
                            .When(x => x.CompanyId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SalesOrganisationCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateSalesOrganisationCommand.SalesOrganisationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOrganisationCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
