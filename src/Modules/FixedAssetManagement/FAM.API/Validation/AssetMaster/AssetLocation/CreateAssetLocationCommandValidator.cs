using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetLocation
{
    public class CreateAssetLocationCommandValidator  : AbstractValidator<CreateAssetLocationCommand>
    { 
      private readonly List<ValidationRule> _validationRules;
        public CreateAssetLocationCommandValidator()
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
                       /*  RuleFor(x => x.AssetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.AssetId)} {rule.Error}")
                             .Must(BeNumeric)
                            .WithMessage($"{nameof(CreateAssetLocationCommand.AssetId)} must be a valid number."); */
                        When(x => x.AssetId != 0, () =>
                        {
                            RuleFor(x => x.AssetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.AssetId)} is required.");
                        });     
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.UnitId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(CreateAssetLocationCommand.UnitId)} must be a valid number.");
                         RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.DepartmentId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(CreateAssetLocationCommand.DepartmentId)} must be a valid number.");    
                        RuleFor(x => x.LocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.LocationId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(CreateAssetLocationCommand.LocationId)} must be a valid number.");
                            RuleFor(x => x.SubLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetLocationCommand.SubLocationId)} {rule.Error}")
                            .Must(BeNumeric)
                            .WithMessage($"{nameof(CreateAssetLocationCommand.SubLocationId)} must be a valid number.");
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