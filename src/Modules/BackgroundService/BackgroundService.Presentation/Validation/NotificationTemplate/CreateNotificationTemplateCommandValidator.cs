using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationTemplate
{
    public class CreateNotificationTemplateCommandValidator  : AbstractValidator<CreateNotificationTemplateCommand>
    {
        private readonly List<ValidationRule> _validationRules;                    
        private readonly INotificationTemplateCommandRepository _NotificationTemplateCommandRepository;

        public CreateNotificationTemplateCommandValidator(MaxLengthProvider maxLengthProvider,INotificationTemplateCommandRepository NotificationTemplateCommandRepository)
        {            
            _NotificationTemplateCommandRepository = NotificationTemplateCommandRepository;
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
                        RuleFor(x => x.SubjectTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.SubjectTemplate)} {rule.Error}");
                        RuleFor(x => x.NotificationTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.NotificationTypeId)} {rule.Error}");
                        RuleFor(x => x.NotificationConfigId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.NotificationConfigId)} {rule.Error}");
                        RuleFor(x => x.BodyTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.BodyTemplate)} {rule.Error}");     
                        RuleFor(x => x.HeaderTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.HeaderTemplate)} {rule.Error}");
                        RuleFor(x => x.LanguageCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationTemplateCommand.LanguageCode)} {rule.Error}");                        
                        break;
                    case "AlreadyExists":   
                        RuleFor(x => x.NotificationConfigId)                    
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateNotificationTemplateCommand.NotificationConfigId)} {rule.Error}")
                        .MustAsync(async (command, notificationConfigId, cancellation) =>
                            !await _NotificationTemplateCommandRepository
                                .ExistsByCodeAsync(notificationConfigId, command.NotificationTypeId, command.LanguageCode))
                        .WithMessage("The combination already exists.");
                        break;



                }
            }
        }
    }
}