using FluentValidation;
using Core.Domain.Entities;
using Core.Application.Users.Commands.CreateUser;
using Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.RoleEntitlements
{
    public class CreateRoleEntitlementCommandValidator : AbstractValidator<CreateRoleEntitlementCommand>
    {
      private readonly List<ValidationRule> _validationRules;
      public CreateRoleEntitlementCommandValidator(MaxLengthProvider maxLengthProvider)
      {
            // _validationRules = ValidationRuleLoader.LoadValidationRules();
            // if (_validationRules == null || !_validationRules.Any())
            // {
            //     throw new InvalidOperationException("Validation rules could not be loaded.");
            // }

            // foreach (var rule in _validationRules)
            // {
                // switch (rule.Rule)
                // {
                    // case "NotEmpty":
                    //     RuleFor(x => x.RoleName)
                    //         .NotEmpty()
                    //         .WithMessage($"{nameof(CreateRoleEntitlementCommand.RoleName)} {rule.Error}");
                    //     break; 
                    // case "ModuleId":
                    //     RuleForEach(x => x.ModuleMenus).ChildRules(module =>
                    //     {
                    //         module.RuleFor(m => m.ModuleId)
                    //             .GreaterThan(0)
                    //             .WithMessage($"{rule.Error}");

                    //         module.RuleForEach(m => m.Menus).ChildRules(menu =>
                    //         {
                    //             menu.RuleFor(me => me.MenuId)
                    //                 .GreaterThan(0).WithMessage($"{rule.Error}");
                    //         });
                    //     });
                    //     break;
                                  
                    //     default:
                        // Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                    //     break;
                // }
            // }
      }   
    }
}