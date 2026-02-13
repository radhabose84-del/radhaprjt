using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.UOM
{
    public class UpdateUOMCommandValidator: AbstractValidator<UpdateUOMCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateUOMCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var UOMNameMaxLength = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.UOM>("UOMName") ?? 50;

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
            //  Common character validation – only alphanumeric + spaces (no special chars)
            const string alphaNumWithSpacePattern = @"^[A-Za-z0-9 ]+$";

            RuleFor(x => (x.Code ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.Code))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");

            RuleFor(x => (x.UOMName ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.UOMName))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");
        }
        
        
    }
}