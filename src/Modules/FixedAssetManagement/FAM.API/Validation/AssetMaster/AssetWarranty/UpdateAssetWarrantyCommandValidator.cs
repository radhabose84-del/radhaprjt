using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using Shared.Validation; 
using FluentValidation;
using Shared.Validation.Common;
using FAM.API.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetWaranty
{
    public class UpdateAssetWarrantyCommandValidator : AbstractValidator<UpdateAssetWarrantyCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public UpdateAssetWarrantyCommandValidator(MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.Period)} {rule.Error}");
                        RuleFor(x => x.StartDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.StartDate)} {rule.Error}");   
                        RuleFor(x => x.EndDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.EndDate)} {rule.Error}");   
                        RuleFor(x => x.ContactPerson)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ContactPerson)} {rule.Error}");   
                        RuleFor(x => x.MobileNumber)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.MobileNumber)} {rule.Error}");                                                            
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Description)
                            .MaximumLength(assetDescriptionMaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.Description)} {rule.Error} {assetDescriptionMaxLength}"); 
                        RuleFor(x => x.ContactPerson)
                            .MaximumLength(assetContactPersonMaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ContactPerson)} {rule.Error} {assetContactPersonMaxLength}");       
                        RuleFor(x => x.Email)
                            .MaximumLength(assetEmailMaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.Email)} {rule.Error} {assetEmailMaxLength}");   
                        RuleFor(x => x.ServiceAddressLine1)
                            .MaximumLength(assetServiceAddressLine1MaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceAddressLine1)} {rule.Error} {assetServiceAddressLine1MaxLength}"); 
                        RuleFor(x => x.ServiceEmail)
                            .MaximumLength(assetServiceEmailMaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceEmail)} {rule.Error} {assetServiceEmailMaxLength}"); 
                        RuleFor(x => x.ServiceAddressLine2)
                            .MaximumLength(assetServiceAddressLine2MaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceAddressLine2)} {rule.Error} {assetServiceAddressLine2MaxLength}");  
                        RuleFor(x => x.ServiceClaimProcessDescription)
                            .MaximumLength(assetServiceClaimProcessDescriptionMaxLength) 
                            .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceClaimProcessDescription)} {rule.Error} {assetServiceClaimProcessDescriptionMaxLength}");                            
                        break;          
                    case "NumericOnly":       
                        RuleFor(x => x.Period)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.Period)} {rule.Error}");    
                        RuleFor(x => x.WarrantyType)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.WarrantyType)} {rule.Error}");                   
                        RuleFor(x => x.ServiceCountryId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceCountryId)} {rule.Error}");  
                        RuleFor(x => x.ServiceStateId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceStateId)} {rule.Error}");  
                        RuleFor(x => x.ServiceCityId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceCityId)} {rule.Error}");                         
                        break;
                     case "MobileNumber":
                        RuleFor(x => x.MobileNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.MobileNumber)} {rule.Error}");  
                         RuleFor(x => x.ServiceMobileNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceMobileNumber)} {rule.Error}");                        
                        break;  
                    case "Pincode":
                        RuleFor(x => x.ServicePinCode)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServicePinCode)} {rule.Error}");
                          break;
                    case "Email":
                        RuleFor(x => x.Email)
                        .EmailAddress()
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.Email)} {rule.Error}");
                         RuleFor(x => x.ServiceEmail)
                        .EmailAddress()
                        .WithMessage($"{nameof(UpdateAssetWarrantyCommand.ServiceEmail)} {rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}