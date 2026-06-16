using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.DeleteTaxCodeMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TaxCode
{
    public class DeleteTaxCodeMasterCommandValidator : AbstractValidator<DeleteTaxCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITaxCodeQueryRepository _queryRepository;

        public DeleteTaxCodeMasterCommandValidator(ITaxCodeQueryRepository queryRepository)
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
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteTaxCodeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.TaxCodeNotFoundAsync(id))
                            .WithMessage($"Tax Code {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.TaxCodeLinkedAsync(id))
                            .WithMessage("Cannot delete — code is linked to GL account(s). Unlink first.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
