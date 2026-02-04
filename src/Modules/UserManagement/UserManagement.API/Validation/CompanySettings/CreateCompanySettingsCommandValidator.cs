using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.API.Validation.Common;
using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using FluentValidation;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.CompanySettings
{
    public class CreateCompanySettingsCommandValidator : AbstractValidator<CreateCompanySettingsCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICompanyQuerySettings _icompanyQuerySettings;

        public CreateCompanySettingsCommandValidator( MaxLengthProvider maxLengthProvider, ICompanyQuerySettings icompanyQuerySettings)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _icompanyQuerySettings = icompanyQuerySettings;
            if (_validationRules == null || !_validationRules.Any())    
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            } 

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NumericOnly":
                        RuleFor(x => x.CompanyId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.PasswordHistoryCount)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordHistoryCount)} {rule.Error}");

                        RuleFor(x => x.SessionTimeout)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.SessionTimeout)} {rule.Error}");

                        RuleFor(x => x.FailedLoginAttempts)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.FailedLoginAttempts)} {rule.Error}");

                        RuleFor(x => x.AutoReleaseTime)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.AutoReleaseTime)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryDays)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordExpiryDays)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryAlert)
                        .InclusiveBetween(1, int.MaxValue)    
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordExpiryAlert)} {rule.Error}");

                        RuleFor(x => x.MaxConcurrentLogins)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.MaxConcurrentLogins)} {rule.Error}");

                        RuleFor(x => x.ForgotPasswordCodeExpiry)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.ForgotPasswordCodeExpiry)} {rule.Error}");

                        RuleFor(x => x.Currency)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.Currency)} {rule.Error}");

                        RuleFor(x => x.Language)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.Language)} {rule.Error}");

                        RuleFor(x => x.TimeZone)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.TimeZone)} {rule.Error}");

                        RuleFor(x => x.FinancialYear)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.FinancialYear)} {rule.Error}");
                        
                    break;

                    case "MinLength":
                        RuleFor(x => x.CompanyId)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.PasswordHistoryCount)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordHistoryCount)} {rule.Error}");

                        RuleFor(x => x.SessionTimeout)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.SessionTimeout)} {rule.Error}");

                        RuleFor(x => x.FailedLoginAttempts)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.FailedLoginAttempts)} {rule.Error}");

                        RuleFor(x => x.AutoReleaseTime)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.AutoReleaseTime)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryDays)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordExpiryDays)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryAlert)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.PasswordExpiryAlert)} {rule.Error}");

                        RuleFor(x => x.MaxConcurrentLogins)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.MaxConcurrentLogins)} {rule.Error}");

                        RuleFor(x => x.ForgotPasswordCodeExpiry)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.ForgotPasswordCodeExpiry)} {rule.Error}");

                        RuleFor(x => x.Currency)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.Currency)} {rule.Error}");

                        RuleFor(x => x.Language)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.Language)} {rule.Error}");

                        RuleFor(x => x.TimeZone)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.TimeZone)} {rule.Error}");

                        RuleFor(x => x.FinancialYear)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.FinancialYear)} {rule.Error}");

                        break;

                    case "ByteValue":
                        RuleFor(x => x.TwoFactorAuth)
                        .Must(value => value == 0 || value == 1) 
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.TwoFactorAuth)} {rule.Error}");

                        RuleFor(x => x.CaptchaOnLogin)
                        .Must(value => value == 0 || value == 1) 
                        .WithMessage($"{nameof(CreateCompanySettingsCommand.CaptchaOnLogin)} {rule.Error}");
                     break;
                       case "AlreadyExists":
                           RuleFor(x => x.CompanyId)
                           .MustAsync(async (CompanyId, cancellation) => !await _icompanyQuerySettings.AlreadyExistsAsync(CompanyId))
                           .WithName("Company")
                            .WithMessage($"{rule.Error}");
                            break;
                     default:
                     break;
                }           
            }  
        }
    }
}