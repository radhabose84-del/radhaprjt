using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using FluentValidation;
using UserManagement.Application.Common.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Users
{
    public class ExistingUserPasswordChangeCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IChangePassword _ichangePassword;
        public ExistingUserPasswordChangeCommandValidator( IChangePassword ichangePassword)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _ichangePassword = ichangePassword;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.OldPassword)
                            .NotEmpty()
                            .WithMessage($"{nameof(ChangeUserPasswordCommand.OldPassword)} {rule.Error}");
                        RuleFor(x => x.NewPassword)
                            .NotEmpty()
                            .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}");
                        RuleFor(x => x.UserId)
                            .NotEmpty()
                            .WithMessage($"{nameof(ChangeUserPasswordCommand.UserId)} {rule.Error}");
                     
                        break;

                     case "NumericOnly":
                        RuleFor(x => x.UserId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.UserId)} {rule.Error}");
                        break;

                    case "PasswordMaxLength":
                        RuleFor(x => x.NewPassword)
                        .Length(8,10)
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}"); 
                        break;
                    case "UpperCase":
                        RuleFor(x => x.NewPassword)
                        .Matches(@"[A-Z]+")
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}");
                        break;
                    case "LowerCase":
                        RuleFor(x => x.NewPassword)
                        .Matches(@"[a-z]+")
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}");
                        break;
                    case "Numeric":
                        RuleFor(x => x.NewPassword)
                        .Matches(@"[0-9]+")
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}");
                        break;
                    case "SpecialCharacter":
                        RuleFor(x => x.NewPassword)
                        .Matches(@"[!@#$%^&*(),.?""{}|<>]")
                        .WithMessage($"{nameof(ChangeUserPasswordCommand.NewPassword)} {rule.Error}");
                        break;
                        
                       case "PasswordHistory":
                         RuleFor(x => x.NewPassword)
                      .MustAsync(async (command,newPassword, cancellation) => 
                    !await _ichangePassword.ValidatePassword(command.UserId, newPassword))
                        .WithMessage($"{rule.Error}");
                        break;
                          case "FirsttimeUser":
                         RuleFor(x => x.UserId)
                      .MustAsync(async (UserId, cancellation) => !await _ichangePassword.ValidateFirstTimeUser(UserId))
                        .WithMessage($"{rule.Error}");
                        break;
                        case "OldPassword":
                         RuleFor(x => x.OldPassword)
                      .MustAsync(async (command, oldPassword, cancellation) =>
                         {
                             var userPasswordHash = await _ichangePassword.GetUserPasswordHashAsync(command.UserId);

                             if (string.IsNullOrEmpty(userPasswordHash))
                                 return false; 

                             return BCrypt.Net.BCrypt.Verify(oldPassword, userPasswordHash); 
                         })
                        .WithMessage($"{rule.Error}");
                        break;

                      default:
                        
                        break;
                }
            }
        }
      
    }
}