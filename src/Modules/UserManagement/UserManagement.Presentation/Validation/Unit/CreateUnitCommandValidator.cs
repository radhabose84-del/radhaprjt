#nullable disable
using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Unit
{
    public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
    {
         private readonly List<ValidationRule> _validationRules;
         private readonly IUnitQueryRepository _queryRepo;
         private readonly IBankAccountLookup _bankAccountLookup;

        public CreateUnitCommandValidator(MaxLengthProvider maxLengthProvider, IUnitQueryRepository queryRepo, IBankAccountLookup bankAccountLookup)
        {
            _queryRepo = queryRepo;
            _bankAccountLookup = bankAccountLookup;
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
                        // Apply NotEmpty validation
                        RuleFor(x => x.UnitName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.UnitName)} {rule.Error}");
                        RuleFor(x => x.ShortName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.ShortName)} {rule.Error}");
                        RuleFor(x => x.CompanyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.CompanyId)} {rule.Error}");
                        RuleFor(x => x.DivisionId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.DivisionId)} {rule.Error}");
                        RuleFor(x => x.UnitHeadName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.UnitHeadName)} {rule.Error}");
                        RuleFor(x => x.CINNO)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUnitCommand.CINNO)} {rule.Error}");
                         RuleFor(x => x.UnitAddressDto.CountryId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.CountryId)} {rule.Error}");
                        RuleFor(x => x.UnitAddressDto.StateId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.StateId)} {rule.Error}");
                            RuleFor(x => x.UnitAddressDto.CityId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.CityId)} {rule.Error}");
                        RuleFor(x => x.UnitAddressDto.AddressLine1)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine1)} {rule.Error}");
                        RuleFor(x => x.UnitAddressDto.PinCode.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.PinCode)} {rule.Error}");
                        RuleFor(x => x.UnitAddressDto.ContactNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error}");
                        RuleFor(x => x.UnitContactsDto.Name)
                            .NotEmpty()     
                            .WithMessage($"{nameof(UnitContactsDto.Name)} {rule.Error}");    
                        RuleFor(x => x.UnitContactsDto.Designation)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.Designation)} {rule.Error}");
                        RuleFor(x => x.UnitContactsDto.Email)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error}");
                        RuleFor(x => x.UnitContactsDto.PhoneNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error}");
                        RuleFor(x => x.UnitTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateUnitCommand.UnitTypeId)} {rule.Error}");
                        break;
                    case "FKColumnDelete":
                        RuleFor(x => x.UnitTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateUnitCommand.UnitTypeId)} is inactive or deleted.")
                            .When(x => x.UnitTypeId > 0);
                        RuleFor(x => x.BankAccountId)
                            .MustAsync(async (id, ct) => await _bankAccountLookup.ExistsForOwnerTypeAsync(id.Value, "Unit", ct))
                            .WithMessage($"{nameof(CreateUnitCommand.BankAccountId)} is invalid or not a Unit bank account.")
                            .When(x => x.BankAccountId > 0);
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.UnitName)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.UnitName)} {rule.Error} {50}");
                        RuleFor(x => x.ShortName)
                            .MaximumLength(10) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.ShortName)} {rule.Error} {10}");
                        RuleFor(x => x.CompanyId.ToString())
                            .MaximumLength(4) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.CompanyId)} {rule.Error} {4}");
                        RuleFor(x => x.DivisionId.ToString())
                            .MaximumLength(4) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.DivisionId)} {rule.Error} {4}");
                        RuleFor(x => x.UnitHeadName)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.UnitHeadName)} {rule.Error} {50}");
                        RuleFor(x => x.CINNO)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateUnitCommand.CINNO)} {rule.Error} {50}");
                        RuleFor(x => x.UnitAddressDto.AddressLine1)
                            .MaximumLength(250)
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine1)} {rule.Error} {250}");
                        RuleFor(x => x.UnitAddressDto.AddressLine2)
                            .MaximumLength(250)
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine2)} {rule.Error} {250}");
                        RuleFor(x => x.UnitAddressDto.CountryId.ToString())
                            .MaximumLength(4)
                            .WithMessage($"{nameof(UnitAddressDto.CountryId)} {rule.Error} {4}");
                        RuleFor(x => x.UnitAddressDto.StateId.ToString())
                            .MaximumLength(4)
                    .       WithMessage($"{nameof(UnitAddressDto.StateId)} {rule.Error} {4}");
                        RuleFor(x => x.UnitAddressDto.CityId.ToString())
                            .MaximumLength(4)
                    .       WithMessage($"{nameof(UnitAddressDto.CityId)} {rule.Error} {4}");
                        RuleFor(x => x.UnitAddressDto.PinCode.ToString())
                            .MaximumLength(10)
                            .WithMessage($"{nameof(UnitAddressDto.PinCode)} {rule.Error} {10}");
                        RuleFor(x => x.UnitAddressDto.ContactNumber)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error} {40}");
                        RuleFor(x => x.UnitAddressDto.AlternateNumber)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitAddressDto.AlternateNumber)} {rule.Error} {40}"); 

                        RuleFor(x => x.UnitContactsDto.Name)
                            .MaximumLength(50) 
                            .WithMessage($"{nameof(UnitContactsDto.Name)} {rule.Error} {50}"); 
                        RuleFor(x => x.UnitContactsDto.Designation)
                            .MaximumLength(50)
                            .WithMessage($"{nameof(UnitContactsDto.Designation)} {rule.Error} {50}");
                        RuleFor(x => x.UnitContactsDto.Email)
                            .MaximumLength(200)
                            .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error} {200}");
                        RuleFor(x => x.UnitContactsDto.PhoneNo)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error} {40}");
                        RuleFor(x => x.UnitContactsDto.Remarks)
                            .MaximumLength(250)
                            .When(x => !string.IsNullOrEmpty(x.UnitContactsDto.Remarks))
                            .WithMessage($"{nameof(UnitContactsDto.Remarks)} {rule.Error} {250}");
                        break;

                    case "MobileNumber": 
                        RuleFor(x => x.UnitAddressDto.ContactNumber) 
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error}"); 
                        RuleFor(x => x.UnitAddressDto.AlternateNumber)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .When(x => !string.IsNullOrEmpty(x.UnitAddressDto.AlternateNumber))
                            .WithMessage($"{nameof(UnitAddressDto.AlternateNumber)} {rule.Error}"); 
                        RuleFor(x => x.UnitContactsDto.PhoneNo) 
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error}"); 
                     break;
                     case "Email":
                        RuleFor(x => x.UnitContactsDto.Email) 
                            .EmailAddress() 
                            .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error}");
                     break;
                     case "Pincode":
                        RuleFor(x => x.UnitAddressDto.PinCode.ToString()) 
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(UnitAddressDto.PinCode)} {rule.Error}");
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

