using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class CreateTaxAccountLinkageCommandValidator : AbstractValidator<CreateTaxAccountLinkageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public CreateTaxAccountLinkageCommandValidator(ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                        RuleFor(x => x.CompanyId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateTaxAccountLinkageCommand.CompanyId)} {rule.Error}");
                        RuleFor(x => x.TaxCodeId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateTaxAccountLinkageCommand.TaxCodeId)} {rule.Error}");
                        RuleFor(x => x.GlAccountId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateTaxAccountLinkageCommand.GlAccountId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.TaxCodeId)
                            .MustAsync(async (id, ct) => await _queryRepository.TaxCodeExistsAsync(id))
                            .WithMessage($"{nameof(CreateTaxAccountLinkageCommand.TaxCodeId)} {rule.Error}")
                            .When(x => x.TaxCodeId > 0);

                        RuleFor(x => x.GlAccountId)
                            .MustAsync(async (id, ct) => await _queryRepository.GlAccountExistsAsync(id))
                            .WithMessage($"{nameof(CreateTaxAccountLinkageCommand.GlAccountId)} {rule.Error}")
                            .When(x => x.GlAccountId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
