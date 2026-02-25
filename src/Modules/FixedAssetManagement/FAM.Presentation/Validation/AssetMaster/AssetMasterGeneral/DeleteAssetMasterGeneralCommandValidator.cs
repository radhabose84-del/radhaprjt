
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral
{
    public class DeleteAssetMasterGeneralCommandValidator : AbstractValidator<DeleteAssetMasterGeneralCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetMasterGeneralQueryRepository _assetQueryRepository;
        public DeleteAssetMasterGeneralCommandValidator( IAssetMasterGeneralQueryRepository assetQueryRepository)
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
                            .WithMessage($"{nameof(DeleteAssetMasterGeneralCommand.Id)} {rule.Error}");
                        break;
                    case "Positive":                         
                        RuleFor(x => x.Id)
                        .GreaterThan(0)
                        .WithMessage("ID must be greater than 0.");
                    break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                        .MustAsync(async (Id, cancellation) => !await _assetQueryRepository.GetAssetChildDetails(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
            
        }
    }
}