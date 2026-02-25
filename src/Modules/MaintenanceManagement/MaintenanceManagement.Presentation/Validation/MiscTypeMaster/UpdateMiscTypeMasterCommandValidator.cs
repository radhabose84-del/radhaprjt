#nullable disable
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandValidator  : AbstractValidator<UpdateMiscTypeMasterCommand>
    {

          private readonly List<ValidationRule> _validationRules;
            private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
          public UpdateMiscTypeMasterCommandValidator(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository,MaxLengthProvider maxLengthProvider)
          {
             var MiscTypeCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MiscTypeMaster>("MiscTypeCode") ?? 50;
             var DescriptionMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MiscTypeMaster>("Description")?? 250;
               
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
                            #pragma warning disable CS0162
                            .WithMessage("MiscTypeCode already exists."); break;    
                            #pragma warning restore CS0162
                        #pragma warning disable CS0162
                        break;
                        #pragma warning restore CS0162
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