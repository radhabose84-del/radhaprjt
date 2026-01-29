using FAM.Application.Manufacture.Commands.UpdateManufacture;
using FAM.Domain.Entities;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.Manufacture
{
    public class UpdateManufactureCommandValidator : AbstractValidator<UpdateManufactureCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public UpdateManufactureCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider
            var manufactureCodeMaxLength = maxLengthProvider.GetMaxLength<Manufactures>("Code")??10;
            var manufactureNameMaxLength = maxLengthProvider.GetMaxLength<Manufactures>("ManufactureName")??50;            
            var addressLine1MaxLength = maxLengthProvider.GetMaxLength<Manufactures>("AddressLine1")??250;   
            var addressLine2MaxLength = maxLengthProvider.GetMaxLength<Manufactures>("AddressLine2")??250;   
            var pinCodeMaxLength = maxLengthProvider.GetMaxLength<Manufactures>("PinCode")??10;   
            var personMaxLength = maxLengthProvider.GetMaxLength<Manufactures>("PersonName")??50;   
            var phoneMaxLength = maxLengthProvider.GetMaxLength<Manufactures>("PhoneNumber")??50;   

            // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.ManufactureName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.ManufactureName)} {rule.Error}");
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.Code)} {rule.Error}");
                        RuleFor(x => x.ManufactureType)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.ManufactureType)} {rule.Error}");
                        RuleFor(x => x.CountryId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.CountryId)} {rule.Error}");
                        RuleFor(x => x.StateId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.StateId)} {rule.Error}");
                        RuleFor(x => x.CityId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.CityId)} {rule.Error}");
                        RuleFor(x => x.AddressLine1)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.AddressLine1)} {rule.Error}");                        
                        RuleFor(x => x.AddressLine2)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.AddressLine2)} {rule.Error}");  
                        RuleFor(x => x.PinCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.PinCode)} {rule.Error}");  
                        RuleFor(x => x.PersonName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.PersonName)} {rule.Error}");
                        RuleFor(x => x.PhoneNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateManufactureCommand.PhoneNumber)} {rule.Error}");                        
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.ManufactureName)
                            .MaximumLength(manufactureNameMaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.ManufactureName)} {rule.Error} {manufactureNameMaxLength}");
                        RuleFor(x => x.Code)
                            .MaximumLength(manufactureCodeMaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.Code)} {rule.Error} {manufactureCodeMaxLength}");
                         RuleFor(x => x.AddressLine2)
                            .MaximumLength(addressLine2MaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.AddressLine2)} {rule.Error} {addressLine2MaxLength}");
                        RuleFor(x => x.AddressLine1)
                            .MaximumLength(addressLine1MaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.AddressLine1)} {rule.Error} {addressLine1MaxLength}");
                        RuleFor(x => x.PinCode)
                            .MaximumLength(pinCodeMaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.PinCode)} {rule.Error} {pinCodeMaxLength}");
                        RuleFor(x => x.PersonName)
                            .MaximumLength(personMaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.PersonName)} {rule.Error} {personMaxLength}");
                        RuleFor(x => x.PhoneNumber)
                            .MaximumLength(phoneMaxLength) 
                            .WithMessage($"{nameof(UpdateManufactureCommand.PhoneNumber)} {rule.Error} {phoneMaxLength}");
                        break;                           
                    case "Telephone":
                        RuleFor(x => x.PhoneNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(UpdateManufactureCommand.PhoneNumber)} {rule.Error}");                        
                        break;  
                    case "Pincode":
                        RuleFor(x => x.PinCode)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(UpdateManufactureCommand.PinCode)} {rule.Error}");
                          break;
                    case "Email":
                        RuleFor(x => x.Email)
                        .EmailAddress()
                        .WithMessage($"{nameof(UpdateManufactureCommand.Email)} {rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }      
    }
}