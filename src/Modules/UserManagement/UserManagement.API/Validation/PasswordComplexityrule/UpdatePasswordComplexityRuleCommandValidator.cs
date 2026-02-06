using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.API.Validation.Common;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Domain.Entities;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.PasswordComplexityrule
{
    public class UpdatePasswordComplexityRuleCommandValidator :AbstractValidator<UpdatePasswordComplexityRuleCommand>

    {

            private readonly List<ValidationRule> _validationRules;
        
    

     public UpdatePasswordComplexityRuleCommandValidator(MaxLengthProvider maxLengthProvider)
           {

               var PwdComplexityRuleNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.PasswordComplexityRule>("PwdComplexityRule") ?? 150;

               _validationRules = ValidationRuleLoader.LoadValidationRules();

               if (_validationRules == null || !_validationRules.Any())
               {
                   throw new ArgumentException("Validation rules could not be loaded.");                
               }

               foreach (var rule in _validationRules)
               {
                  
                   switch (rule.Rule)
                   {
                       #region NotEmpty
                       case "NotEmpty":
                           // Apply NotEmpty validation rule
                           RuleFor(x => x.PwdComplexityRule)
                               .NotEmpty()
                               .WithMessage($"{nameof(UpdatePasswordComplexityRuleCommand.PwdComplexityRule)} {rule.Error}");
                           break;
                             case "MaxLength":
                RuleFor(x => x.PwdComplexityRule)
                    .MaximumLength(maxLengthProvider.GetMaxLength<PasswordComplexityRule>("PwdComplexityRule") ?? 150)
                    .WithMessage($"{nameof(UpdatePasswordComplexityRuleCommand.PwdComplexityRule)} {rule.Error} ");
                break;
            default:
                Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                break;
                       #endregion
                   }
               }
           }
               

}

    
}