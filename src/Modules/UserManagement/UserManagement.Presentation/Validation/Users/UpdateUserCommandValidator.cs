#nullable disable
using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.Application.Users.Commands.UpdateUser;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUser;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Users
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserQueryRepository _userQueryRepository;

        public UpdateUserCommandValidator(MaxLengthProvider maxLengthProvider,IUserQueryRepository userRepository)
        {
            var MaxLen = maxLengthProvider.GetMaxLength<User>("FirstName") ?? 25;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _userQueryRepository = userRepository;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.FirstName)
                            .NotNull()
                             .WithMessage($"{nameof(UpdateUserCommand.FirstName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUserCommand.FirstName)} {rule.Error}");

                        RuleFor(x => x.UserName)
                            .NotNull()
                             .WithMessage($"{nameof(UpdateUserCommand.UserName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUserCommand.UserName)} {rule.Error}");

                             RuleFor(x => x.UserId)
                             .NotNull()
                             .WithMessage($"{nameof(UpdateUserCommand.UserId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUserCommand.UserId)} {rule.Error}");

                            RuleFor(x => x.UserGroupId)
                             .NotNull()
                             .WithMessage($"{nameof(UpdateUserCommand.UserGroupId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUserCommand.UserGroupId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.FirstName)
                            .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUserCommand.FirstName)} {rule.Error} {MaxLen}");   

                             RuleFor(x => x.LastName)
                            .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUserCommand.LastName)} {rule.Error} {MaxLen}"); 

                             RuleFor(x => x.UserName)
                            .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUserCommand.UserName)} {rule.Error} {MaxLen}"); 
                        break; 
                         
                    case "Email":
                        RuleFor(x => x.EmailId)
                            .EmailAddress()
                            .WithMessage($"{nameof(UpdateUserCommand.EmailId)} {rule.Error}");   
                        break; 

                    case "MobileNumber": 
                        RuleFor(x => x.Mobile) 
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                        .WithMessage($"{nameof(UpdateUserCommand.Mobile)} {rule.Error}"); 
                        break; 

                   
                    
                    case "AlreadyExists":
                           RuleFor(x =>  new { x.UserName, x.UserId })
                           .MustAsync(async (user, cancellation) =>
                        !await _userQueryRepository.AlreadyExistsAsync(user.UserName, user.UserId))
                           .WithName("User Name")
                            .WithMessage($"{rule.Error}");

                        // One UserId per EmpId — allow same-self update, block re-mapping to another User's EmpId
                        RuleFor(x => new { x.EmpId, x.UserId })
                        .MustAsync(async (pair, cancellation) =>
                            !await _userQueryRepository.EmpIdAlreadyExistsAsync(pair.EmpId!.Value, pair.UserId))
                        .WithName("EmpId")
                        .WithMessage(x => $"A user is already mapped to EmpId '{x.EmpId}'. One Employee can have only one User account.")
                        .When(x => x.EmpId.HasValue && x.EmpId.Value > 0);
                            break;
                    case "NotFound":
                           RuleFor(x => x.UserId )
                           .MustAsync(async (UserId, cancellation) => 
                        await _userQueryRepository.NotFoundAsync(UserId))             
                           .WithName("User Id")
                            .WithMessage($"{rule.Error}");
                            break;          
                    default:
                        // Handle unknown rule (log or throw)
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}