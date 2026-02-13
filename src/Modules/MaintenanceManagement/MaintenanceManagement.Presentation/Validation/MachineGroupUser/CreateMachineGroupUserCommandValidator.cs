
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MachineGroupUser
{
    public class CreateMachineGroupUserCommandValidator  : AbstractValidator<CreateMachineGroupUserCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineGroupUserQueryRepository _machineGroupUserQuery;
        public CreateMachineGroupUserCommandValidator(MaxLengthProvider maxLengthProvider,IMachineGroupUserQueryRepository machineGroupUserQuery)
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
                        RuleFor(x => x.MachineGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineGroupUserCommand.MachineGroupId)} {rule.Error}");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineGroupUserCommand.DepartmentId)} {rule.Error}");
                        RuleFor(x => x.UserId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineGroupUserCommand.UserId)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                           RuleFor(x =>  new { x.MachineGroupId,x.DepartmentId,x.UserId })
                           .MustAsync(async (shift, cancellation) => 
                        !await _machineGroupUserQuery.AlreadyExistsAsync(shift.MachineGroupId,shift.DepartmentId,shift.UserId))             
                           .WithName("Machine Group")
                            .WithMessage($"{rule.Error}");
                            break;                    
                      default:                        
                    break;
                }
            }
        }
       
    }
}