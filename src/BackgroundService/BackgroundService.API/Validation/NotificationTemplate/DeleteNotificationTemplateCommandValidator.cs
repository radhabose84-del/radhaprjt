using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using FluentValidation;

namespace BackgroundService.API.Validation.NotificationTemplate
{
    public class DeleteNotificationTemplateCommandValidator : AbstractValidator<DeleteNotificationTemplateCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationTemplateQueryRepository _NotificationTemplateQueryRepository;        

        public DeleteNotificationTemplateCommandValidator(INotificationTemplateQueryRepository NotificationTemplateQueryRepository)
        {
            _NotificationTemplateQueryRepository = NotificationTemplateQueryRepository;            
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
                            .WithMessage($"{nameof(DeleteNotificationTemplateCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => 
                                await _NotificationTemplateQueryRepository.NotFoundAsync(Id))             
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _NotificationTemplateQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}