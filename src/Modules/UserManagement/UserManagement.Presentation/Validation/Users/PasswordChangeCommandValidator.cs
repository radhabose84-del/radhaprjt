using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.Users
{
    public class PasswordChangeCommandValidator : AbstractValidator<FirstTimeUserPasswordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public PasswordChangeCommandValidator(MaxLengthProvider maxLengthProvider)
        {
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
                        RuleFor(x => x.Password)
                            .NotEmpty()
                            .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}");
                        RuleFor(x => x.UserId)
                            .NotEmpty()
                            .WithMessage($"{nameof(FirstTimeUserPasswordCommand.UserId)} {rule.Error}");
                        break;

                    case "PasswordMaxLength":
                        RuleFor(x => x.Password)
                        .Length(8,10)
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}"); 
                        break;
                    case "UpperCase":
                        RuleFor(x => x.Password)
                        .Matches(@"[A-Z]+")
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}");
                        break;
                        case "LowerCase":
                        RuleFor(x => x.Password)
                        .Matches(@"[a-z]+")
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}");
                        break;
                         case "Numeric":
                        RuleFor(x => x.Password)
                        .Matches(@"[0-9]+")
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}");
                        break;
                         case "SpecialCharacter":
                        RuleFor(x => x.Password)
                        .Matches(@"[!@#$%^&*(),.?""{}|<>]")
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.Password)} {rule.Error}");
                        break;
                     case "NumericOnly":
                        RuleFor(x => x.UserId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(FirstTimeUserPasswordCommand.UserId)} {rule.Error}");
                        break;
                  
                    default:
                        
                        break;
                }
            }
        }
    }
}