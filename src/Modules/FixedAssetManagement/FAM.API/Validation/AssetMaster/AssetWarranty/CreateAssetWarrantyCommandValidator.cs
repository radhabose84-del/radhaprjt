using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetWarranty
{
    public class CreateAssetWarrantyCommandValidator : AbstractValidator<CreateAssetWarrantyCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public CreateAssetWarrantyCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider            
            var assetDescriptionMaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("Description")??1000;            
            var assetContactPersonMaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("ContactPerson")??50;   
            var assetEmailMaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("Email")??100;   
            var assetServiceAddressLine1MaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("ServiceAddressLine1")??100;   
            var assetServiceAddressLine2MaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("ServiceAddressLine2")??100;   
            var assetServiceEmailMaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("ServiceEmail")??100;   
            var assetServiceClaimProcessDescriptionMaxLength = maxLengthProvider.GetMaxLength<AssetWarranties>("ServiceClaimProcessDescription")??100;   

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
                        RuleFor(x => x.Period)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.Period)} {rule.Error}");
                        RuleFor(x => x.StartDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.StartDate)} {rule.Error}");   
                        RuleFor(x => x.EndDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.EndDate)} {rule.Error}");   
                        RuleFor(x => x.ContactPerson)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ContactPerson)} {rule.Error}");   
                        RuleFor(x => x.MobileNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.MobileNumber)} {rule.Error}");                                                            
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Description)
                            .MaximumLength(assetDescriptionMaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.Description)} {rule.Error} {assetDescriptionMaxLength}"); 
                        RuleFor(x => x.ContactPerson)
                            .MaximumLength(assetContactPersonMaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ContactPerson)} {rule.Error} {assetContactPersonMaxLength}");       
                        RuleFor(x => x.Email)
                            .MaximumLength(assetEmailMaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.Email)} {rule.Error} {assetEmailMaxLength}");   
                        RuleFor(x => x.ServiceAddressLine1)
                            .MaximumLength(assetServiceAddressLine1MaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceAddressLine1)} {rule.Error} {assetServiceAddressLine1MaxLength}"); 
                        RuleFor(x => x.ServiceEmail)
                            .MaximumLength(assetServiceEmailMaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceEmail)} {rule.Error} {assetServiceEmailMaxLength}"); 
                        RuleFor(x => x.ServiceAddressLine2)
                            .MaximumLength(assetServiceAddressLine2MaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceAddressLine2)} {rule.Error} {assetServiceAddressLine2MaxLength}");  
                        RuleFor(x => x.ServiceClaimProcessDescription)
                            .MaximumLength(assetServiceClaimProcessDescriptionMaxLength) 
                            .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceClaimProcessDescription)} {rule.Error} {assetServiceClaimProcessDescriptionMaxLength}");                            
                        break;          
                    case "NumericOnly":       
                        RuleFor(x => x.Period)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.Period)} {rule.Error}");    
                        RuleFor(x => x.WarrantyType)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.WarrantyType)} {rule.Error}");                   
                        RuleFor(x => x.ServiceCountryId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceCountryId)} {rule.Error}");  
                        RuleFor(x => x.ServiceStateId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceStateId)} {rule.Error}");  
                        RuleFor(x => x.ServiceCityId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceCityId)} {rule.Error}");                         
                        break;
                     case "MobileNumber":
                        RuleFor(x => x.MobileNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.MobileNumber)} {rule.Error}");  
                         RuleFor(x => x.ServiceMobileNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceMobileNumber)} {rule.Error}");                        
                        break;  
                    case "Pincode":
                        RuleFor(x => x.ServicePinCode)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServicePinCode)} {rule.Error}");
                          break;
                    case "Email":
                        RuleFor(x => x.Email)
                        .EmailAddress()
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.Email)} {rule.Error}");
                         RuleFor(x => x.ServiceEmail)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateAssetWarrantyCommand.ServiceEmail)} {rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}