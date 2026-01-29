using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using FluentValidation;

namespace BackgroundService.API.Validation.NotificationTemplate
{
    public class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
    {        
        private readonly List<ValidationRule> _validationRules;                    
        private readonly INotificationTemplateCommandRepository _NotificationTemplateCommandRepository;
        private readonly INotificationTemplateQueryRepository _NotificationTemplateQueryRepository;  
        public UpdateNotificationTemplateCommandValidator(MaxLengthProvider maxLengthProvider, INotificationTemplateCommandRepository NotificationTemplateCommandRepository,INotificationTemplateQueryRepository NotificationTemplateQueryRepository)
        {
            _NotificationTemplateQueryRepository = NotificationTemplateQueryRepository;            
            _NotificationTemplateCommandRepository = NotificationTemplateCommandRepository;           

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
                       RuleFor(x => x.SubjectTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.SubjectTemplate)} {rule.Error}");
                        RuleFor(x => x.NotificationTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.NotificationTypeId)} {rule.Error}");
                        RuleFor(x => x.NotificationConfigId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.NotificationConfigId)} {rule.Error}");
                        RuleFor(x => x.BodyTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.BodyTemplate)} {rule.Error}");
                        RuleFor(x => x.HeaderTemplate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.HeaderTemplate)} {rule.Error}");
                        RuleFor(x => x.LanguageCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationTemplateCommand.LanguageCode)} {rule.Error}");  
                        break;                  
                    case "AlreadyExists":
                         RuleFor(x => x.NotificationConfigId)                    
                        .NotEmpty()
                        .WithMessage($"{nameof(UpdateNotificationTemplateCommand.NotificationConfigId)} {rule.Error}")
                        .MustAsync(async (command, notificationConfigId, cancellation) =>
                            !await _NotificationTemplateCommandRepository
                                .IsNameDuplicateAsync(notificationConfigId, command.NotificationTypeId, command.LanguageCode,command.Id))
                        .WithMessage("The combination already exists.");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) =>
                                await _NotificationTemplateQueryRepository.NotFoundAsync(Id))
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