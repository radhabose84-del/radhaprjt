using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationConfig
{
    public class UpdateNotificationConfigCommandValidator : AbstractValidator<UpdateNotificationConfigCommand>
    {        
        private readonly List<ValidationRule> _validationRules;                    
        private readonly INotificationConfigCommandRepository _notificationConfigCommandRepository;
        private readonly INotificationConfigQueryRepository _notificationConfigQueryRepository;  
        public UpdateNotificationConfigCommandValidator(MaxLengthProvider maxLengthProvider, INotificationConfigCommandRepository notificationConfigCommandRepository,INotificationConfigQueryRepository notificationConfigQueryRepository)
        {
            _notificationConfigQueryRepository = notificationConfigQueryRepository;            
            _notificationConfigCommandRepository = notificationConfigCommandRepository;
            var maxLength = maxLengthProvider.GetMaxLength<Domain.Entities.Notification.NotificationConfig>("ModuleName") ?? 250;

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
                        RuleFor(x => x.ModuleName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationConfigCommand.ModuleName)} {rule.Error}");
                        RuleFor(x => x.NotificationEventTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationConfigCommand.NotificationEventTypeId)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.ModuleName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateNotificationConfigCommand.ModuleName)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => x.ModuleName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationConfigCommand.ModuleName)} {rule.Error}")
                            .MustAsync(async (command, moduleName, cancellation) =>
                            !await _notificationConfigCommandRepository.IsNameDuplicateAsync(moduleName, command.NotificationEventTypeId,command.Id))
                            .WithMessage("A ModuleName already exists in this Event Type.");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) =>
                                await _notificationConfigQueryRepository.NotFoundAsync(Id))
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}