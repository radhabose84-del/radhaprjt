using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.UserLogin.Commands.UserLogin;
using UserManagement.Domain.Entities;
using FluentValidation;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.Common.Interfaces;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.UserLogin
{
    public class UserLoginCommandValidator: AbstractValidator<UserLoginCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly ICompanyQuerySettings _companyQuerySettings;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IIPAddressService _ipAddressService;
        public UserLoginCommandValidator(MaxLengthProvider maxLengthProvider, IUserQueryRepository userQueryRepository, ICompanyQuerySettings companyQuerySettings,IUserSessionRepository userSessionRepository,IIPAddressService iPAddressService)
        {
            // CascadeMode = CascadeMode.Stop;
            var MaxLen = maxLengthProvider.GetMaxLength<User>("UserName") ?? 25;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _companyQuerySettings = companyQuerySettings;
            _userQueryRepository = userQueryRepository;
            _userSessionRepository = userSessionRepository;
            _ipAddressService = iPAddressService;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Username)
                            .NotNull()
                             .WithMessage($"{nameof(UserLoginCommand.Username)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UserLoginCommand.Username)} {rule.Error}");
                         RuleFor(x => x.Password)
                             .NotNull()
                             .WithMessage($"{nameof(UserLoginCommand.Password)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UserLoginCommand.Password)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Username)
                            .MaximumLength(MaxLen) 
                            .WithMessage($"{nameof(UserLoginCommand.Username)} {rule.Error} {MaxLen}");   
                        break; 
 
                    case "NotFound":
                           RuleFor(x => x.Username )
                           .Cascade(CascadeMode.Stop)
                           .MustAsync(async (Username, cancellation) => 
                        await _userQueryRepository.AlreadyExistsAsync(Username))             
                           .WithName("User Name")
                            .WithMessage($"{rule.Error}");
                        //     .MustAsync(async (Username, cancellation) => 
                        // await _companyQuerySettings.BeforeLoginNotFoundValidation(Username))             
                        //    .WithName("User Name")
                        //     .WithMessage("User Admin Settings not found")
                        //     .When(x => _ipAddressService.GetGroupcode() =="USER")
                        //     .MustAsync(async (Username, cancellation) => 
                        // await _userQueryRepository.ValidateUserRolesAsync(Username))
                        //     .WithMessage("User does not have any role assigned to it. Contact your admin.");
                            
                           
                            break;
                            
                    case "UserSession":
                     RuleSet("UserSession", () => 
                      {
                           RuleFor(x => x.Username )
                           .MustAsync(async (Username, cancellation) => 
                        !await _userSessionRepository.ValidateUserSession(Username))
                        .WhenAsync(async (request, cancellation) =>
            await ValidatePassword(request.Username, request.Password))
                            .WithMessage($"{rule.Error}");
                    });
                            break;  
                            
                    // case "UserRole":
                    //        RuleFor(x => x.Username )
                    //        .MustAsync(async (Username, cancellation) => 
                    //     await _userQueryRepository.ValidateUserRolesAsync(Username))
                    //         .WithMessage($"{rule.Error}");
                    //         break;
                    case "UserLock":
                           RuleFor(x => x.Username)
                           .MustAsync(async (UserName, cancellation) => !await _userQueryRepository.UserLockedAsync(UserName))
                           .WithName("User Name")
                            .WithMessage($"{rule.Error}");
                            break; 
                            
                    default:                        
                        break;
                }
            }
        }
         private async Task<bool> ValidatePassword(string userName, string Password)
           {
               var user = await _userQueryRepository.GetByUsernameAsync(userName);
               if (user != null)
               {
                   return BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash);
               }
               return false;
           }
    }
}