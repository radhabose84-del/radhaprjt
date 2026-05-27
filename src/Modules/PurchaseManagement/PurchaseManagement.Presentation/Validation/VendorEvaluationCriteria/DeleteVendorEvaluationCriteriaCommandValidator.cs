using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria
{
    public class DeleteVendorEvaluationCriteriaCommandValidator : AbstractValidator<DeleteVendorEvaluationCriteriaCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;

        public DeleteVendorEvaluationCriteriaCommandValidator(IVendorEvaluationCriteriaQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteVendorEvaluationCriteriaCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"VendorEvaluationCriteria {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
