using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using UserManagement.Application.UserRole.Commands.UpdateRole;
using UserManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.UserRole
{
    public class UpdateRoleCommandValidator:AbstractValidator<UpdateRoleCommand>
    {

         private readonly List<ValidationRule> _validationRules;
        public UpdateRoleCommandValidator (MaxLengthProvider maxLengthProvider)
           {

                    var DepartmentShortNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserRole>("RoleName") ?? 50;
                   var DepartmentDeptNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserRole>("Description") ?? 250;            
             _validationRules = ValidationRuleLoader.LoadValidationRules();
             if (_validationRules == null || !_validationRules.Any())
             {
                 throw new InvalidOperationException("Validation rules could not be loaded.");
             }  

             // Loop through the rules and apply them        

           foreach (var rule in _validationRules)
                   {
                       switch(rule.Rule)
                       {
                           case "NotEmpty":
                               // Apply NotEmpty validation
                               RuleFor(x => x.RoleName).MaximumLength(DepartmentShortNameMaxLength)
                                   .NotEmpty()
                                   .WithMessage($"{nameof(UpdateRoleCommand.RoleName)} {rule.Error}");
                               RuleFor(x => x.Description).MaximumLength(DepartmentDeptNameMaxLength)
                                   .NotEmpty()
                                   .WithMessage($"{nameof(UpdateRoleCommand.Description)} {rule.Error}");
                               break;
                           case "MaxLength":
                               // Apply MaxLength validation using dynamic max length values
                               RuleFor(x => x.RoleName).MaximumLength(DepartmentShortNameMaxLength)
                                   .WithMessage($"{nameof(UpdateRoleCommand.RoleName)} {rule.Error}");
                               RuleFor(x => x.Description).MaximumLength(DepartmentDeptNameMaxLength)
                                   .WithMessage($"{nameof(UpdateRoleCommand.Description)} {rule.Error}");
                               break;
                       }
                   }


           }
        
    }
}