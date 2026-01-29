using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MachineGroupUser
{
    public class UpdateMachineGroupUserCommandValidator  : AbstractValidator<UpdateMachineGroupUserCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineGroupUserQueryRepository _machineGroupUserQuery;
        public UpdateMachineGroupUserCommandValidator(MaxLengthProvider maxLengthProvider,IMachineGroupUserQueryRepository machineGroupUserQuery)
        {             
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _machineGroupUserQuery =machineGroupUserQuery;

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
                            .WithMessage($"{nameof(UpdateMachineGroupUserCommand.Id)} {rule.Error}");
                        RuleFor(x => x.MachineGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineGroupUserCommand.MachineGroupId)} {rule.Error}");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineGroupUserCommand.DepartmentId)} {rule.Error}");
                        RuleFor(x => x.UserId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineGroupUserCommand.UserId)} {rule.Error}");
                        break;                   
                    case "AlreadyExists":
                           RuleFor(x =>  new { x.MachineGroupId,x.DepartmentId,x.UserId, x.Id })
                           .MustAsync(async (shift, cancellation) => 
                        !await _machineGroupUserQuery.AlreadyExistsAsync(shift.MachineGroupId,shift.DepartmentId,shift.UserId,shift.Id))             
                           .WithName("Machine Group")
                            .WithMessage($"{rule.Error}");
                            break; 
                     case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _machineGroupUserQuery.NotFoundAsync(Id))             
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