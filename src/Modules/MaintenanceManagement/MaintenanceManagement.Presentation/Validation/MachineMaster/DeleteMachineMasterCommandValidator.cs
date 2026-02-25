using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MachineMaster
{
    public class DeleteMachineMasterCommandValidator : AbstractValidator<DeleteMachineMasterCommand> 
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly IMachineMasterQueryRepository _iMachineMasterQueryRepository;
        public DeleteMachineMasterCommandValidator(IMachineMasterQueryRepository iMachineMasterQueryRepository)
        {
            _iMachineMasterQueryRepository = iMachineMasterQueryRepository;
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
                            .WithMessage($"{nameof(DeleteMachineMasterCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iMachineMasterQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                    // case "SoftDelete":
                    //      RuleFor(x => x.Id)
                    //   .MustAsync(async (Id, cancellation) => !await _iCostCenterQueryRepository.SoftDeleteValidation(Id))
                    //     .WithMessage($"{rule.Error}");
                    //     break;
                    default:
                        
                        break;
                }
            }
        }
    }
}