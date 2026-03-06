using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationConfig
{
    public class CreateNotificationConfigCommandValidator  : AbstractValidator<CreateNotificationConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;                    
        private readonly INotificationConfigCommandRepository _notificationConfigCommandRepository;

        public CreateNotificationConfigCommandValidator(MaxLengthProvider maxLengthProvider,INotificationConfigCommandRepository notificationConfigCommandRepository)
        {
            var maxLength = maxLengthProvider.GetMaxLength<Domain.Entities.Notification.NotificationConfig>("ModuleName") ?? 250;
            
            _notificationConfigCommandRepository = notificationConfigCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new ArgumentException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ModuleName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationConfigCommand.ModuleName)} {rule.Error}");
                        RuleFor(x => x.NotificationEventTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationConfigCommand.NotificationEventTypeId)} {rule.Error}");
                        break;
                    case "MaxLength":                        
                        RuleFor(x => x.ModuleName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateNotificationConfigCommand.ModuleName)} {rule.Error}");
                        break;
                    case "AlreadyExists":                       
                        RuleFor(x => x.ModuleName)
                           .NotEmpty()
                           .WithMessage($"{nameof(CreateNotificationConfigCommand.ModuleName)} {rule.Error}")
                           .MustAsync(async (command, moduleName, cancellation) =>
                            !await _notificationConfigCommandRepository.ExistsByCodeAsync(moduleName, command.NotificationEventTypeId))
                             .WithMessage("A ModuleName already exists in this Event Type.");
                        break;
                }
            }
        }
    }
}