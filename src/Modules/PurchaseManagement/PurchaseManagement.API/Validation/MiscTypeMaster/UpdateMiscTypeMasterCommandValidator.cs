using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FluentValidation;
using PurchaseManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.API.Validation.MiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandValidator  : AbstractValidator<UpdateMiscTypeMasterCommand>
    { 

          private readonly List<ValidationRule> _validationRules;
            private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
          public UpdateMiscTypeMasterCommandValidator(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository,MaxLengthProvider maxLengthProvider)
          {
             var MiscTypeCodeMaxLength = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.MiscTypeMaster>("MiscTypeCode") ?? 50;
             var DescriptionMaxLength = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.MiscTypeMaster>("Description")?? 250;
               
               _validationRules = ValidationRuleLoader.LoadValidationRules();
               _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
             if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty" :
                     // Apply NotEmpty validation
                        RuleFor(x => x.MiscTypeCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;
                        case "MaxLength" :
                        RuleFor(x => x.MiscTypeCode)
                           
                            .MaximumLength(MiscTypeCodeMaxLength)
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.MiscTypeCode)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateMiscTypeMasterCommand.Description)} {rule.Error}");
                        break;
                        case "AlreadyExists":
                         RuleFor(x => x)
                            .MustAsync(async (command, cancellation) =>
                                !await _miscTypeMasterQueryRepository.AlreadyExistsAsync(command.MiscTypeCode, command.Id))
                            .WithName(nameof(UpdateMiscTypeMasterCommand.MiscTypeCode))
                            .WithMessage("MiscTypeCode already exists.");  
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _miscTypeMasterQueryRepository.NotFoundAsync(Id))             
                           .WithName("MiscTypeCode Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                     
                }

           }
          }
        
    }
}