using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PurchaseIndent
{
    public class DeletePurchaseIndentCommandValidator : AbstractValidator<DeletePurchaseIndentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPurchaseIndentQuery _queryRepository;

        public DeletePurchaseIndentCommandValidator(IPurchaseIndentQuery queryRepository)
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
                            .WithMessage($"{nameof(DeletePurchaseIndentCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"PurchaseIndent {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
