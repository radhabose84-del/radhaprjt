using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Commands.DeleteStoReceipt;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoReceipt
{
    public class DeleteStoReceiptCommandValidator : AbstractValidator<DeleteStoReceiptCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoReceiptQueryRepository _queryRepository;

        public DeleteStoReceiptCommandValidator(IStoReceiptQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteStoReceiptCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"STO Receipt {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
