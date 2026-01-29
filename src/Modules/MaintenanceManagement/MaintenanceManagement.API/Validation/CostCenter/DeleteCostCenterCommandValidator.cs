using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.CostCenter
{
    public class DeleteCostCenterCommandValidator : AbstractValidator<DeleteCostCenterCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;

        public DeleteCostCenterCommandValidator(ICostCenterQueryRepository iCostCenterQueryRepository)
        {
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
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
                            .WithMessage($"{nameof(DeleteCostCenterCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iCostCenterQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _iCostCenterQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        
                        break;
                }
            }
        }
    }
}