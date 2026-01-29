using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using FluentValidation;
using UserManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.DepartmentGroup
{
    public class UpdateDepartmentGroupCommandValidator : AbstractValidator<UpdateDepartmentGroupCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public UpdateDepartmentGroupCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var GroupCodeMaxLength = maxLengthProvider.GetMaxLength<Core.Domain.Entities.DepartmentGroup>("DepartmentGroupCode") ?? 15;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<Core.Domain.Entities.DepartmentGroup>("DepartmentGroupName") ?? 50;

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
                        // Apply NotEmpty validation
                        RuleFor(x => x.DepartmentGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDepartmentGroupCommand.DepartmentGroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.DepartmentGroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(UpdateDepartmentGroupCommand.DepartmentGroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;
                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }




    }
}