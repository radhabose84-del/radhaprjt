using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceCategory
{
    public class DeleteMaintenanceCategoryCommandValidator : AbstractValidator<DeleteMaintenanceCategoryCommand> 
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceCategoryQueryRepository _iMaintenanceCategoryQueryRepository;
         public DeleteMaintenanceCategoryCommandValidator(IMaintenanceCategoryQueryRepository iMaintenanceCategoryQueryRepository)
        {
            _iMaintenanceCategoryQueryRepository = iMaintenanceCategoryQueryRepository;
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
                            .WithMessage($"{nameof(DeleteMaintenanceCategoryCommand.Id)} {rule.Error}");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                (await _iMaintenanceCategoryQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}")
                            .When(x => x.Id > 0);
                            break;
                    default:
                        
                        break;
                }
            }
        }
    }
}