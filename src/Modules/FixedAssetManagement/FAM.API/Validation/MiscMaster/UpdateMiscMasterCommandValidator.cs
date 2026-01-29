using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.MiscMaster
{
    public class UpdateMiscMasterCommandValidator : AbstractValidator<UpdateMiscMasterCommand>
    {
    

        private readonly List<ValidationRule> _validationRules;
          public UpdateMiscMasterCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var MiscCodeMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.MiscMaster>("Code") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.MiscMaster>("Description")?? 250;

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
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}");
                        RuleFor(x => x.MiscTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.MiscTypeId)} {rule.Error}");
                       


                        break;
                        case "MaxLength" :
                        RuleFor(x => x.Code)
                           
                            .MaximumLength(MiscCodeMaxLength)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}");
                        break;
                }

            }
       

        }

    }
}