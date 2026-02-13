using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Presentation.Validation.Common;
using FAM.Infrastructure.Data;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidator  : AbstractValidator<CreateMiscTypeMasterCommand>
    {


             private readonly List<ValidationRule> _validationRules;
      public CreateMiscTypeMasterCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var MiscTypeCodeMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.MiscTypeMaster>("MiscTypeCode") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.MiscTypeMaster>("Description")?? 250;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                      throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
             switch (rule.Rule)
                {
                    case "NotFound" :
                     // Apply NotEmpty validation
                        RuleFor(x => x.MiscTypeCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;
                        case "MaxLength" :
                        RuleFor(x => x.MiscTypeCode)
                           
                            .MaximumLength(MiscTypeCodeMaxLength)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(CreateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;
                }

            }
       

            }


        
    }
}