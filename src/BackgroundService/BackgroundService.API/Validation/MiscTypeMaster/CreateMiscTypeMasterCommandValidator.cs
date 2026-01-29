
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FluentValidation;

namespace BackgroundService.API.Validation.MiscTypeMaster
{
    public class CreateMiscTypeMasterCommandValidator  : AbstractValidator<CreateMiscTypeMasterCommand>
    {
             private readonly List<ValidationRule> _validationRules;
             private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
      public CreateMiscTypeMasterCommandValidator( IMiscTypeMasterQueryRepository machineGroupQueryRepository,MaxLengthProvider maxLengthProvider)
        {
            var MiscTypeCodeMaxLength = maxLengthProvider.GetMaxLength<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>("MiscTypeCode") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>("Description")?? 250;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _miscTypeMasterQueryRepository = machineGroupQueryRepository;
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
                    case "AlreadyExists":
                        RuleFor(x => x.MiscTypeCode)
                            .MustAsync(async (miscTypeCode, cancellation) =>
                                !await _miscTypeMasterQueryRepository.AlreadyExistsAsync(miscTypeCode))
                            .WithMessage("MiscTypeCode already exists.");
                        break;    


                }

            }
       

            }
        
    }
}