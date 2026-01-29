using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MaintenanceCategory
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
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iMaintenanceCategoryQueryRepository.GetByIdAsync(id)) != null) 
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