using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceType
{
    public class DeleteMaintenanceTypeCommandValidator : AbstractValidator<DeleteMaintenanceTypeCommand>  
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceTypeQueryRepository _imaintenanceTypeQueryRepository;
         public DeleteMaintenanceTypeCommandValidator(IMaintenanceTypeQueryRepository imaintenanceTypeQueryRepository)
        {
            _imaintenanceTypeQueryRepository = imaintenanceTypeQueryRepository;
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
                            .WithMessage($"{nameof(DeleteMaintenanceTypeCommand.Id)} {rule.Error}");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                (await _imaintenanceTypeQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}")
                            .When(x => x.Id > 0);
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