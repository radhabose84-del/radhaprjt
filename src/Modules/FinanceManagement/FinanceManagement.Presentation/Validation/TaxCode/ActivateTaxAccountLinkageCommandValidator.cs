using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class ActivateTaxAccountLinkageCommandValidator : AbstractValidator<ActivateTaxAccountLinkageCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public ActivateTaxAccountLinkageCommandValidator(ITaxCodeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty().WithMessage($"{nameof(ActivateTaxAccountLinkageCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.LinkageNotFoundAsync(id))
                            .WithMessage($"Tax-account linkage {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // AC2-B: cannot activate without a GL account mapping.
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await _queryRepository.LinkageHasGlMappingAsync(id))
                .WithMessage("Tax code cannot be activated without a GL account mapping.");
        }
    }
}
