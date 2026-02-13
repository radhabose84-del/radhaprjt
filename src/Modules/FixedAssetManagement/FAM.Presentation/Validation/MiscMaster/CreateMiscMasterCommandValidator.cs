using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.MiscMaster
{
    public class CreateMiscMasterCommandValidator  : AbstractValidator<CreateMiscMasterCommand>
    {
            private readonly List<ValidationRule> _validationRules;

         public CreateMiscMasterCommandValidator(MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}");
                              RuleFor(x => x.MiscTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} {rule.Error}");
                        break;
                        case "MaxLength" :
                        RuleFor(x => x.Code)
                           
                            .MaximumLength(MiscCodeMaxLength)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}");
                        break;
                        //  case "AlreadyExists":
                        // RuleFor(x => x)
                        //     .MustAsync(async (command, cancellation) =>
                        //     {
                        //         return !await _miscMasterQuery.AlreadyExistsAsync(command.Code, command.MiscTypeId);
                        //     })
                        //     .WithMessage($"{rule.Error}")
                        //     .WithName("Misc Code");
                        // break; 
                }

            }
       

        }
        
    }
}