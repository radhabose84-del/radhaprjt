using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using UserManagement.Application.Departments.Commands;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Domain.Entities;
using UserManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Department
{
    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
           private readonly List<ValidationRule> _validationRules;

           public CreateDepartmentCommandValidator(MaxLengthProvider maxLengthProvider)
           {
                    var DepartmentShortNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Department>("ShortName") ?? 6;
                   var DepartmentDeptNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Department>("DeptName") ?? 50;            

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
                            RuleFor(x => x.DeptName).MaximumLength(DepartmentDeptNameMaxLength)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateDepartmentCommand.DeptName)} {rule.Error}");
                            RuleFor(x => x.ShortName).MaximumLength(DepartmentShortNameMaxLength)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateDepartmentCommand.ShortName)} {rule.Error}");
                            break;

                            case "MaxLength":
                            // Apply MaxLength validation using dynamic max length values
                            RuleFor(x => x.DeptName)
                                .MaximumLength(DepartmentDeptNameMaxLength)
                                .WithMessage($"{nameof(CreateDepartmentCommand.DeptName)} {rule.Error} {DepartmentDeptNameMaxLength}");
                            RuleFor(x => x.ShortName)
                            .MaximumLength(DepartmentDeptNameMaxLength)
                            .WithMessage($"{nameof(CreateDepartmentCommand.ShortName)} {rule.Error} {DepartmentShortNameMaxLength}"); break;
                            default:
                               // Handle unknown rule (log or throw)
                               Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                            break;

                       }
                   }


           }

    }
}