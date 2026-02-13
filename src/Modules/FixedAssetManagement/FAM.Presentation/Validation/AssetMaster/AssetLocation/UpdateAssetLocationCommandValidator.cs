using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetLocation
{
    public class UpdateAssetLocationCommandValidator  : AbstractValidator<UpdateAssetLocationCommand>
    {
        
          private readonly List<ValidationRule> _validationRules;
          public UpdateAssetLocationCommandValidator()
        {
            
        

           _validationRules = new List<ValidationRule>();
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new ArgumentException("Validation rules could not be loaded.");

            }

            foreach (var rule in _validationRules)
            {
                  switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.AssetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.AssetId)} {rule.Error}")
                             .Must(BeNumeric)
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.AssetId)} must be a valid number.");

                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.UnitId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.UnitId)} must be a valid number.");
                         RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.DepartmentId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.DepartmentId)} must be a valid number.");    
                        RuleFor(x => x.LocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.LocationId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.LocationId)} must be a valid number.");
                            RuleFor(x => x.SubLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.SubLocationId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.SubLocationId)} must be a valid number.");
                        break;
                        
                  }
              

            }
        }
          private bool BeNumeric(int value)
        {
            return value >= 0; // Ensures only positive numbers are allowed
        }

    }
}