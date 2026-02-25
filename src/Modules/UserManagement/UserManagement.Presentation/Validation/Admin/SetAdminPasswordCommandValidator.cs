#nullable disable
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Utilities;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Admin
{
    public class SetAdminPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITimeZoneService _timeZoneService;
        public SetAdminPasswordCommandValidator(ITimeZoneService timeZoneService)
        {
                 _validationRules = ValidationRuleLoader.LoadValidationRules();
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
                            // Apply NotEmpty validation
                            RuleFor(x => x.Email)
                                .NotEmpty()
                                .WithMessage($"{nameof(ResetPasswordCommand.Email)} {rule.Error}")
                                .Must(EmailExistsInCache)
                                .WithMessage("Verification code is invalid or has expired.");
                            RuleFor(x => x.VerificationCode)
                                .NotEmpty()
                                .WithMessage($"{nameof(ResetPasswordCommand.VerificationCode)} {rule.Error}")
                                .Must(CodeMatches)
                                .WithMessage("Invalid verification code.");

                            RuleFor(x => x)
                                .Must(NotExpired)
                                .WithMessage("Verification code has expired.");
                                
                            break;  
                }
                
           }
        }
    private bool EmailExistsInCache(string email)
    {
        return ForgotPasswordCache.CodeStorage.ContainsKey(email);
    }

    private bool CodeMatches(ResetPasswordCommand request, string code)
    {
        if (ForgotPasswordCache.CodeStorage.TryGetValue(request.Email, out var verificationCodeDetails))
        {
            return verificationCodeDetails.Code == code;
        }
        return false;
    }

    private bool NotExpired(ResetPasswordCommand request)
    {
        if (ForgotPasswordCache.CodeStorage.TryGetValue(request.Email, out var verificationCodeDetails))
        {
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            if (verificationCodeDetails.ExpiryTime < currentTime)
            {
                // Remove expired code from cache
                ForgotPasswordCache.CodeStorage.Remove(request.Email);
                return false;
            }
        }
        return true;
    }
    }
}