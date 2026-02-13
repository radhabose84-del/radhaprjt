using FluentValidation;
using UserManagement.Application.Units.Commands.CreateUnit;
using System.Text.RegularExpressions;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Infrastructure.Migrations;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;
namespace UserManagement.Presentation.Validation.Unit
{
    public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateUnitCommandValidator(MaxLengthProvider maxLengthProvider)
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
                        // Apply NotEmpty validation
                        RuleFor(x => x.UpdateUnitDto.UnitName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.UnitName)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.ShortName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.ShortName)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.CompanyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.CompanyId)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.DivisionId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.DivisionId)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitHeadName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.UnitHeadName)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.CINNO)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.CINNO)} {rule.Error}");
                         RuleFor(x => x.UpdateUnitDto.UnitAddressDto.CountryId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.CountryId)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.StateId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.StateId)} {rule.Error}");
                            RuleFor(x => x.UpdateUnitDto.UnitAddressDto.CityId.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.CityId)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.AddressLine1)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine1)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.PinCode.ToString())
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.PinCode)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.ContactNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Name)
                            .NotEmpty()     
                            .WithMessage($"{nameof(UnitContactsDto.Name)} {rule.Error}");    
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Designation)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.Designation)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Email)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.PhoneNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.UpdateUnitDto.UnitName)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.UnitName)} {rule.Error} {50}");
                        RuleFor(x => x.UpdateUnitDto.ShortName)
                            .MaximumLength(10) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.ShortName)} {rule.Error} {10}");
                        RuleFor(x => x.UpdateUnitDto.CompanyId.ToString())
                            .MaximumLength(4) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.CompanyId)} {rule.Error} {4}");
                        RuleFor(x => x.UpdateUnitDto.DivisionId.ToString())
                            .MaximumLength(4) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.DivisionId)} {rule.Error} {4}");
                        RuleFor(x => x.UpdateUnitDto.UnitHeadName)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.UnitHeadName)} {rule.Error} {50}");
                        RuleFor(x => x.UpdateUnitDto.CINNO)
                            .MaximumLength(50) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(UpdateUnitCommand.UpdateUnitDto.CINNO)} {rule.Error} {50}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.AddressLine1)
                            .MaximumLength(250)
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine1)} {rule.Error} {250}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.AddressLine2)
                            .MaximumLength(250)
                            .WithMessage($"{nameof(UnitAddressDto.AddressLine2)} {rule.Error} {250}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.CountryId.ToString())
                            .MaximumLength(4)
                            .WithMessage($"{nameof(UnitAddressDto.CountryId)} {rule.Error} {4}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.StateId.ToString())
                            .MaximumLength(4)
                    .       WithMessage($"{nameof(UnitAddressDto.StateId)} {rule.Error} {4}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.CityId.ToString())
                            .MaximumLength(4)
                    .       WithMessage($"{nameof(UnitAddressDto.CityId)} {rule.Error} {4}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.PinCode.ToString())
                            .MaximumLength(10)
                            .WithMessage($"{nameof(UnitAddressDto.PinCode)} {rule.Error} {10}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.ContactNumber)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error} {40}");
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.AlternateNumber)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitAddressDto.AlternateNumber)} {rule.Error} {40}"); 

                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Name)
                            .MaximumLength(50) 
                            .WithMessage($"{nameof(UnitContactsDto.Name)} {rule.Error} {50}"); 
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Designation)
                            .MaximumLength(50)
                            .WithMessage($"{nameof(UnitContactsDto.Designation)} {rule.Error} {50}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Email)
                            .MaximumLength(200)
                            .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error} {200}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.PhoneNo)
                            .MaximumLength(40)
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error} {40}");
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Remarks)
                            .MaximumLength(250)
                            .When(x => !string.IsNullOrEmpty(x.UpdateUnitDto.UnitContactsDto.Remarks))
                            .WithMessage($"{nameof(UnitContactsDto.Remarks)} {rule.Error} {250}");
                        break;

                    case "MobileNumber": 
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.ContactNumber) 
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(UnitAddressDto.ContactNumber)} {rule.Error}"); 
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.AlternateNumber)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .When(x => !string.IsNullOrEmpty(x.UpdateUnitDto.UnitAddressDto.AlternateNumber))
                            .WithMessage($"{nameof(UnitAddressDto.AlternateNumber)} {rule.Error}"); 
                        RuleFor(x => x.UpdateUnitDto.UnitContactsDto.PhoneNo) 
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(UnitContactsDto.PhoneNo)} {rule.Error}"); 
                     break;
                     case "Email":
                RuleFor(x => x.UpdateUnitDto.UnitContactsDto.Email) 
                    .EmailAddress() 
                    .WithMessage($"{nameof(UnitContactsDto.Email)} {rule.Error}");
                     break;
                     case "PinCode":
                        RuleFor(x => x.UpdateUnitDto.UnitAddressDto.PinCode.ToString()) 
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