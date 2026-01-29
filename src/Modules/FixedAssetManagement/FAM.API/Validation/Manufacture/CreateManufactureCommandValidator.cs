using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Domain.Entities;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.Manufacture
{
    public class CreateManufactureCommandValidator : AbstractValidator<CreateManufactureCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateManufactureCommandValidator(MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(CreateManufactureCommand.ManufactureName)} {rule.Error}");
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.Code)} {rule.Error}");
                        RuleFor(x => x.ManufactureType)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.ManufactureType)} {rule.Error}");
                        RuleFor(x => x.CountryId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.CountryId)} {rule.Error}");
                        RuleFor(x => x.StateId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.StateId)} {rule.Error}");
                        RuleFor(x => x.CityId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.CityId)} {rule.Error}");
                        RuleFor(x => x.AddressLine1)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.AddressLine1)} {rule.Error}");                        
                        RuleFor(x => x.AddressLine2)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.AddressLine2)} {rule.Error}");  
                        RuleFor(x => x.PinCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.PinCode)} {rule.Error}");  
                        RuleFor(x => x.PersonName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.PersonName)} {rule.Error}");
                        RuleFor(x => x.PhoneNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateManufactureCommand.PhoneNumber)} {rule.Error}");                        
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.ManufactureName)
                            .MaximumLength(manufactureNameMaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.ManufactureName)} {rule.Error} {manufactureNameMaxLength}");
                        RuleFor(x => x.Code)
                            .MaximumLength(manufactureCodeMaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.Code)} {rule.Error} {manufactureCodeMaxLength}");
                         RuleFor(x => x.AddressLine2)
                            .MaximumLength(addressLine2MaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.AddressLine2)} {rule.Error} {addressLine2MaxLength}");
                        RuleFor(x => x.AddressLine1)
                            .MaximumLength(addressLine1MaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.AddressLine1)} {rule.Error} {addressLine1MaxLength}");
                        RuleFor(x => x.PinCode)
                            .MaximumLength(pinCodeMaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.PinCode)} {rule.Error} {pinCodeMaxLength}");
                        RuleFor(x => x.PersonName)
                            .MaximumLength(personMaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.PersonName)} {rule.Error} {personMaxLength}");
                        RuleFor(x => x.PhoneNumber)
                            .MaximumLength(phoneMaxLength) 
                            .WithMessage($"{nameof(CreateManufactureCommand.PhoneNumber)} {rule.Error} {phoneMaxLength}");
                        break;
                    case "MinLength":
                        RuleFor(x => x.CityId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateManufactureCommand.CityId)} {rule.Error} {0}");   
                        RuleFor(x => x.CountryId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateManufactureCommand.CountryId)} {rule.Error} {0}");   
                        RuleFor(x => x.StateId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateManufactureCommand.StateId)} {rule.Error} {0}");   
                        break;
                    case "Telephone":
                        RuleFor(x => x.PhoneNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateManufactureCommand.PhoneNumber)} {rule.Error}");                        
                        break;  
                    case "Pincode":
                        RuleFor(x => x.PinCode)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateManufactureCommand.PinCode)} {rule.Error}");
                          break;
                    case "Email":
                        RuleFor(x => x.Email)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateManufactureCommand.Email)} {rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }            
    }
}