using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.MixCodeMaster
{
    public class DeleteMixCodeMasterCommandValidator : AbstractValidator<DeleteMixCodeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMixCodeMasterQueryRepository _queryRepository;

        public DeleteMixCodeMasterCommandValidator(IMixCodeMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteMixCodeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"MixCodeMaster {rule.Error}");
                        break;

                    case "SoftDelete":
                        // Rule #25 — fixed message (do not customise)
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
