using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.API.Validation.Common;
using Core.Application.CompanySettings.Commands.UpdateCompanySettings;
using FluentValidation;
using Core.Application.Common.Interfaces.ICompanySettings;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.CompanySettings
{
    public class UpdateCompanySettingsCommandValidator : AbstractValidator<UpdateCompanySettingsCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICompanyQuerySettings _icompanyQuerySettings;

        public UpdateCompanySettingsCommandValidator(ICompanyQuerySettings icompanyQuerySettings)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _icompanyQuerySettings = icompanyQuerySettings;

            if(_validationRules == null || !_validationRules.Any())
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
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.PasswordHistoryCount)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordHistoryCount)} {rule.Error}");

                        RuleFor(x => x.SessionTimeout)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.SessionTimeout)} {rule.Error}");

                        RuleFor(x => x.FailedLoginAttempts)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.FailedLoginAttempts)} {rule.Error}");

                        RuleFor(x => x.AutoReleaseTime)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.AutoReleaseTime)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryDays)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordExpiryDays)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryAlert)
                        .InclusiveBetween(1, int.MaxValue)    
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordExpiryAlert)} {rule.Error}");

                        RuleFor(x => x.MaxConcurrentLogins)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.MaxConcurrentLogins)} {rule.Error}");

                        RuleFor(x => x.ForgotPasswordCodeExpiry)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.ForgotPasswordCodeExpiry)} {rule.Error}");

                        RuleFor(x => x.Currency)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.Currency)} {rule.Error}");

                        RuleFor(x => x.Language)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.Language)} {rule.Error}");

                        RuleFor(x => x.TimeZone)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.TimeZone)} {rule.Error}");

                        RuleFor(x => x.FinancialYear)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.FinancialYear)} {rule.Error}");
                        
                    break;

                    case "MinLength":
                        RuleFor(x => x.CompanyId)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.PasswordHistoryCount)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordHistoryCount)} {rule.Error}");

                        RuleFor(x => x.SessionTimeout)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.SessionTimeout)} {rule.Error}");

                        RuleFor(x => x.FailedLoginAttempts)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.FailedLoginAttempts)} {rule.Error}");

                        RuleFor(x => x.AutoReleaseTime)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.AutoReleaseTime)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryDays)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordExpiryDays)} {rule.Error}");

                        RuleFor(x => x.PasswordExpiryAlert)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.PasswordExpiryAlert)} {rule.Error}");

                        RuleFor(x => x.MaxConcurrentLogins)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.MaxConcurrentLogins)} {rule.Error}");

                        RuleFor(x => x.ForgotPasswordCodeExpiry)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.ForgotPasswordCodeExpiry)} {rule.Error}");

                        RuleFor(x => x.Currency)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.Currency)} {rule.Error}");

                        RuleFor(x => x.Language)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.Language)} {rule.Error}");

                        RuleFor(x => x.TimeZone)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.TimeZone)} {rule.Error}");

                        RuleFor(x => x.FinancialYear)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.FinancialYear)} {rule.Error}");

                        break;

                    case "ByteValue":
                        RuleFor(x => x.TwoFactorAuth)
                        .Must(value => value == 0 || value == 1) 
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.TwoFactorAuth)} {rule.Error}");

                        RuleFor(x => x.CaptchaOnLogin)
                        .Must(value => value == 0 || value == 1) 
                        .WithMessage($"{nameof(UpdateCompanySettingsCommand.CaptchaOnLogin)} {rule.Error}");
                     break;
                        case "AlreadyExists":
                           RuleFor(x =>  new { x.Id, x.CompanyId })
                           .MustAsync(async (company, cancellation) => 
                        !await _icompanyQuerySettings.AlreadyExistsAsync(company.CompanyId,company.Id))             
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