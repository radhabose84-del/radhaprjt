#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities;
using UserManagement.Application.Users.Commands.ResetUserPassword;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.Users
{
    public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IChangePassword _ichangePassword;
           private readonly IUserQueryRepository _userQueryRepository;
        private readonly ITimeZoneService _timeZoneService;
        public ResetUserPasswordCommandValidator( IChangePassword ichangePassword,IUserQueryRepository userQueryRepository,ITimeZoneService timeZoneService)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _ichangePassword = ichangePassword;
            _userQueryRepository = userQueryRepository;
            _timeZoneService = timeZoneService;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.UserName)
                            .NotEmpty()
                            .WithMessage($"{nameof(ResetUserPasswordCommand.UserName)} {rule.Error}");
                        RuleFor(x => x.Password)
                            .NotEmpty()
                            .WithMessage($"{nameof(ResetUserPasswordCommand.Password)} {rule.Error}");
                     
                        break;
                    case "PasswordHistory":
                         RuleFor(x => x.Password)
                        .MustAsync(async (command,Password, cancellation) => 
                          !await _ichangePassword.ValidatePasswordbyUserName(command.UserName, Password))
                        .WithMessage($"{rule.Error}");
                        break;
                          case "NotFound":
                         RuleFor(x => x.UserName)
                        .MustAsync((userName, cancellation) =>
                        IsUserValid(userName))
                        .WithMessage($"{rule.Error}");
                        break;
                    case "ExpiredVerificationCode":
                         RuleFor(x => x.UserName)
                         .MustAsync((userName, cancellation) => 
                            IsVerificationCodeValid(userName))
                            .WithMessage($"{rule.Error}");
                        break;
                    case "InvalidVerificationCode":
                         RuleFor(x => x.VerificationCode)
                         .MustAsync((request, verificationCode, cancellation) =>
                            IsVerificationCodeCorrect(request.UserName, verificationCode))
                        .WithMessage($"{rule.Error}");
                        break;
                        case "PasswordCompare":
                         RuleFor(x => x.VerificationCode)
                        .MustAsync(async (command, password, cancellation) =>
                            !await IsNewPasswordSameAsOldPassword(command.UserName, password))
                        .WithMessage($"{rule.Error}");
                        break;
                        //   case "IsFirstime":
                        //  RuleFor(x => x.UserName)
                        //  .MustAsync(async (UserName, cancellation) => await _userQueryRepository.IsFirstimeUserValidation(UserName))
                        // .WithMessage($"{rule.Error}");
                        // break;
                         default:
                       
                        break;
                }
                
            }
        #pragma warning disable CS1998
        }
        #pragma warning restore CS1998
          #pragma warning disable CS1998
          private async Task<bool> IsVerificationCodeValid(string userName)
          #pragma warning restore CS1998
          {
              var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
              var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
         
              if (!ForgotPasswordCache.CodeStorage.TryGetValue(userName, out var verificationCodeDetails))
              {
                  return false;
              }
         
              // Check if the verification code has expired
              if (verificationCodeDetails.ExpiryTime < currentTime)
              {
                  // Remove expired code from the cache
                  ForgotPasswordCache.CodeStorage.Remove(userName);
                  return false;
              }
         
              return true;
          }

    
#pragma warning disable CS1998
    
    
#pragma warning restore CS1998
           #pragma warning disable CS1998
           private async Task<bool> IsVerificationCodeCorrect(string userName, string verificationCode)
           #pragma warning restore CS1998
           {
               if (ForgotPasswordCache.CodeStorage.TryGetValue(userName, out var verificationCodeDetails))
               {
                   return verificationCodeDetails.Code == verificationCode;
               }
               return false;
           }

           
           private async Task<bool> IsUserValid(string userName)
           {
               var user = await _userQueryRepository.GetByUsernameAsync(userName);
               return user != null;
           }

           
           private async Task<bool> IsNewPasswordSameAsOldPassword(string userName, string newPassword)
           {
               var user = await _userQueryRepository.GetByUsernameAsync(userName);
               if (user != null)
               {
                   return BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash);
               }
               return false;
           }
    }
}