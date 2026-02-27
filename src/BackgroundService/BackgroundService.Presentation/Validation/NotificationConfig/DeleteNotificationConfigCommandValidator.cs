using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationConfig
{
    public class DeleteNotificationConfigCommandValidator : AbstractValidator<DeleteNotificationConfigCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationConfigQueryRepository _notificationConfigQueryRepository;        

        public DeleteNotificationConfigCommandValidator(INotificationConfigQueryRepository notificationConfigQueryRepository)
        {
            _notificationConfigQueryRepository = notificationConfigQueryRepository;            
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteNotificationConfigCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => 
                                await _notificationConfigQueryRepository.NotFoundAsync(Id))             
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _notificationConfigQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}