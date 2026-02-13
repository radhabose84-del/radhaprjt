using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetInsurance
{
    public class CreateAssetInsuranceCommandValidator  : AbstractValidator<CreateAssetInsuranceCommand>
    {
          private readonly List<ValidationRule> _validationRules;
          private readonly IAssetInsuranceQueryRepository _assetInsuranceQueryRepository;
            public CreateAssetInsuranceCommandValidator( IAssetInsuranceQueryRepository assetInsuranceQueryRepository)
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
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.AssetId)} {rule.Error}");

                        RuleFor(x => x.PolicyNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.PolicyNo)} {rule.Error}");                             

                        RuleFor(x => x.StartDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.StartDate)} {rule.Error}");
                           // .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");

                          RuleFor(x => x.EndDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.EndDate)} {rule.Error}");
                           // .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

                        RuleFor(x => x.PolicyAmount)                           
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.PolicyAmount)} {rule.Error}");                                                                                                  

                        RuleFor(x => x.VendorCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.VendorCode)} {rule.Error}");

                        RuleFor(x => x.RenewedDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetInsuranceCommand.RenewedDate)} {rule.Error}"); 
                            
                           

                        RuleFor(x => x.RenewalStatus)
                            .NotEmpty().WithMessage($"{nameof(CreateAssetInsuranceCommand.RenewalStatus)} {rule.Error}");
                        
                        break;
                    case "AlreadyExists":
                           RuleFor(x => x.PolicyNo)
                           .MustAsync(async (PolicyNo, cancellation) => !await _assetInsuranceQueryRepository.AlreadyExistsAsync(PolicyNo))
                           .WithName("PolicyNo")
                            .WithMessage($"{rule.Error}");
                            break;

                    case "ActivePolicy":
                           RuleFor(x => x.AssetId)
                           .MustAsync(async (AssetId, cancellation) => !await _assetInsuranceQueryRepository.ActiveInsuranceValidation(AssetId))
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