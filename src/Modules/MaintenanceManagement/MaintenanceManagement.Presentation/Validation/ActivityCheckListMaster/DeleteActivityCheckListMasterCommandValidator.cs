using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ActivityCheckListMaster
{
    public class DeleteActivityCheckListMasterCommandValidator : AbstractValidator<DeleteActivityCheckListMasterCommand>
    {

        private readonly List<ValidationRule> _validationRules;

        private readonly IActivityCheckListMasterQueryRepository  _activityCheckListMasterQueryRepository;

        public DeleteActivityCheckListMasterCommandValidator(IActivityCheckListMasterCommandRepository activityCheckListMasterCommandRepository , IActivityCheckListMasterQueryRepository activityCheckListMasterQueryRepository)
        {
            _activityCheckListMasterQueryRepository = activityCheckListMasterQueryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteActivityCheckListMasterCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                (await _activityCheckListMasterQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _activityCheckListMasterQueryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }

        }
          
    }
}