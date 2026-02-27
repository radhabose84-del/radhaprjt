using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.MiscMaster
{
    public class UpdateMiscMasterCommandValidator  : AbstractValidator<UpdateMiscMasterCommand>
    {
    

        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _miscMasterQuery;

          public UpdateMiscMasterCommandValidator( IMiscMasterQueryRepository miscMasterQuery,MaxLengthProvider maxLengthProvider)
        {
            var MiscCodeMaxLength = maxLengthProvider.GetMaxLength<BackgroundService.Domain.Entities.Notification.MiscMaster>("Code") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<BackgroundService.Domain.Entities.Notification.MiscMaster>("Description")?? 250;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _miscMasterQuery = miscMasterQuery;
            if (_validationRules == null || !_validationRules.Any())
            {
                      throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
             switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Code)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Code)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}");
                        RuleFor(x => x.MiscTypeId)
                            .GreaterThan(0)
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


                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, cancellation) =>
                            {
                                return !await _miscMasterQuery.AlreadyExistsAsync(command.Code, command.MiscTypeId,command.Id);
                            })
                            .WithMessage($"{rule.Error}")
                            .WithName("Misc Code");
                            break; 
                    case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _miscMasterQuery.NotFoundAsync(Id))             
                           .WithName("Misc Code")
                            .WithMessage($"{rule.Error}");
                            break; 
                }

            }
       

        }

        
    }
}