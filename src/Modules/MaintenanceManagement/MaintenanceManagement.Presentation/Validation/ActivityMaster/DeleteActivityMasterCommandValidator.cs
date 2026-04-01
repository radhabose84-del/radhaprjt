using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.DeleteActivityMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ActivityMaster
{
    public class DeleteActivityMasterCommandValidator : AbstractValidator<DeleteActivityMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;

        public DeleteActivityMasterCommandValidator(IActivityMasterQueryRepository activityMasterQueryRepository)
        {
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteActivityMasterCommand.Id)} {rule.Error}");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => await _activityMasterQueryRepository.NotFoundAsync(id))
                            .WithMessage($"ActivityMaster {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _activityMasterQueryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
