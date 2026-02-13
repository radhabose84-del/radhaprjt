using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using Shared.Validation; 
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetWarranty
{
    public class DeleteAssetWarrantyCommandValidator : AbstractValidator<DeleteAssetWarrantyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetWarrantyQueryRepository _assetQueryRepository;
        public DeleteAssetWarrantyCommandValidator( IAssetWarrantyQueryRepository assetQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetQueryRepository = assetQueryRepository;

            
             if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteAssetWarrantyCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _assetQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
            
        }
    }
}