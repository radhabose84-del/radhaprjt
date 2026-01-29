using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MachineGroup
{
    public class DeleteMachineGroupCommandValidator : AbstractValidator<DeleteMachineGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineGroupQueryRepository _machineGroupQueryRepository; 
        public DeleteMachineGroupCommandValidator(IMachineGroupQueryRepository machineGroupQueryRepository)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _machineGroupQueryRepository = machineGroupQueryRepository;
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
                            .WithMessage($"{nameof(DeleteMachineGroupCommand.Id)} {rule.Error}");
                        break;                   
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _machineGroupQueryRepository.NotFoundAsync(Id))             
                           .WithName("MachineGroup Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        } 
    }
}