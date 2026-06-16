using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class SubmitLinkageChangeRequestCommandValidator : AbstractValidator<SubmitLinkageChangeRequestCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public SubmitLinkageChangeRequestCommandValidator(ITaxCodeQueryRepository queryRepository)
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
                            .GreaterThan(0).WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.CompanyId)} {rule.Error}");
                        RuleFor(x => x.GlAccountId)
                            .GreaterThan(0).WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.GlAccountId)} {rule.Error}");
                        RuleFor(x => x.NewTaxCodeId)
                            .GreaterThan(0).WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.NewTaxCodeId)} {rule.Error}");
                        RuleFor(x => x.Reason)
                            .NotNull().WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.Reason)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.Reason)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.NewTaxCodeId)
                            .MustAsync(async (id, ct) => await _queryRepository.TaxCodeExistsAsync(id))
                            .WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.NewTaxCodeId)} {rule.Error}")
                            .When(x => x.NewTaxCodeId > 0);

                        RuleFor(x => x.GlAccountId)
                            .MustAsync(async (id, ct) => await _queryRepository.GlAccountExistsAsync(id))
                            .WithMessage($"{nameof(SubmitLinkageChangeRequestCommand.GlAccountId)} {rule.Error}")
                            .When(x => x.GlAccountId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
