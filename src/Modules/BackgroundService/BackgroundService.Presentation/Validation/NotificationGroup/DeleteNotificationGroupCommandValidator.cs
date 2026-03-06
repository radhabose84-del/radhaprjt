using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationGroup
{

    public class DeleteNotificationGroupCommandValidator : AbstractValidator<DeleteNotificationGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationGroupQuery _notificationGroupQuery;
        public DeleteNotificationGroupCommandValidator(INotificationGroupQuery notificationGroupQuery)
        {
            _notificationGroupQuery = notificationGroupQuery;

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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteNotificationGroupCommand.Id)} {rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _notificationGroupQuery.NotFoundAsync(Id))             
                           .WithName("Notification Group Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
            
        }

    }
}