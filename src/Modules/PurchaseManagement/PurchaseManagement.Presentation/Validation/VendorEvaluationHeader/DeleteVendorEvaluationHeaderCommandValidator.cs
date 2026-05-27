using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationHeader
{
    public class DeleteVendorEvaluationHeaderCommandValidator : AbstractValidator<DeleteVendorEvaluationHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepository;

        public DeleteVendorEvaluationHeaderCommandValidator(IVendorEvaluationHeaderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteVendorEvaluationHeaderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"VendorEvaluationHeader {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
