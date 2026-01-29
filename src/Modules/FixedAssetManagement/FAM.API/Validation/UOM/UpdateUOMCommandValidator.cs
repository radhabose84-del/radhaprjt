using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.UOM
{
    public class UpdateUOMCommandValidator : AbstractValidator<UpdateUOMCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateUOMCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var UOMNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.UOM>("UOMName") ?? 50;

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
                            .WithMessage($"{nameof(UpdateUOMCommand.Code)} {rule.Error}");
                        RuleFor(x => x.UOMName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUOMCommand.UOMName)} {rule.Error}");
                            RuleFor(x => x.SortOrder)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUOMCommand.SortOrder)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.UOMName)
                            .MaximumLength(UOMNameMaxLength)
                            .WithMessage($"{nameof(UpdateUOMCommand.UOMName)} {rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.UOMTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateUOMCommand.UOMTypeId)} {rule.Error} {0}");   
                        break; 
                }
            }
        }
        
    }
}