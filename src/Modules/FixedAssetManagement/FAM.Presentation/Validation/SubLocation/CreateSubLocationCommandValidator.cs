using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.SubLocation
{
    public class CreateSubLocationCommandValidator : AbstractValidator<CreateSubLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateSubLocationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var SubLocationNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.SubLocation>("SubLocationName") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.SubLocation>("Description") ?? 100;
            
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSubLocationCommand.Code)} {rule.Error}");
                        RuleFor(x => x.SubLocationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSubLocationCommand.SubLocationName)} {rule.Error}");
                        // RuleFor(x => x.Description)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateSubLocationCommand.Description)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.SubLocationName)
                            .MaximumLength(SubLocationNameMaxLength)
                            .WithMessage($"{nameof(CreateSubLocationCommand.SubLocationName)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(CreateSubLocationCommand.Description)} {rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateSubLocationCommand.UnitId)} {rule.Error} {0}");   
                            RuleFor(x => x.DepartmentId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateSubLocationCommand.DepartmentId)} {rule.Error} {0}");
                            RuleFor(x => x.LocationId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateSubLocationCommand.LocationId)} {rule.Error} {0}");
                        break; 
                }
            }
        }
    }
}