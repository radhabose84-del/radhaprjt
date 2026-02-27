using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetInsurance
{
    public class UpdateAssetInsuranceCommandValidator  : AbstractValidator<UpdateAssetInsuranceCommand>
    {
         private readonly List<ValidationRule> _validationRules;
         private readonly IAssetInsuranceQueryRepository _assetInsuranceQueryRepository;

         public UpdateAssetInsuranceCommandValidator( IAssetInsuranceQueryRepository assetInsuranceQueryRepository)
         {
             _validationRules = new List<ValidationRule>();
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetInsuranceQueryRepository = assetInsuranceQueryRepository;
                 if (!_validationRules.Any())
            {
                throw new ArgumentException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.AssetId)
                            .NotEmpty().WithMessage("Asset ID is required.");

                        RuleFor(x => x.PolicyNo)
                            .NotEmpty().WithMessage("Policy number is required.");

                        RuleFor(x => x.StartDate)
                            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");

                        RuleFor(x => x.EndDate)
                            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

                        RuleFor(x => x.PolicyAmount)
                            .Must(value => value >= 0) // Ensure it's a valid decimal value
                            .WithMessage("PolicyAmount must be a positive number.");
                            
                        RuleFor(x => x.PolicyAmount)
                        .NotNull().WithMessage("Policy amount is required.")
                        .PrecisionScale(18, 2, true) // precision: 18, scale: 2, ignoreTrailingZeros: true
                        .WithMessage("Policy amount must have up to 2 decimal places and a maximum of 18 digits.");   

                        RuleFor(x => x.VendorCode)
                            .NotEmpty().WithMessage("Vendor code is required.");

                        RuleFor(x => x.RenewedDate)
                             .NotEmpty().WithMessage("Renewed date is required.");

                        RuleFor(x => x.RenewalStatus)
                            .NotEmpty().WithMessage("Renewal status is required.");
                       
                        break;
                        case "AlreadyExists":
                           RuleFor(x => new { x.PolicyNo, x.Id })
                           .MustAsync(async (insurance, cancellation) => !await _assetInsuranceQueryRepository.AlreadyExistsAsync(insurance.PolicyNo ?? string.Empty, insurance.Id))
                           .WithName("PolicyNo")
                            .WithMessage($"{rule.Error}");
                            break;
                         case "ActivePolicy":
                           RuleFor(x => new { x.AssetId, x.Id })
                           .MustAsync(async (insurance, cancellation) => !await _assetInsuranceQueryRepository.ActiveInsuranceValidation(insurance.AssetId, insurance.Id))
                            .WithMessage($"{rule.Error}")
                            .When(x => x.IsActive == 1);
                            break;

                            default:
                            break;

                }
            }    
         }
        
    }
}