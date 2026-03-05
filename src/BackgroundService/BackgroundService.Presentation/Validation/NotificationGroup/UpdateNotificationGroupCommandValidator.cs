using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationGroup
{
    public class UpdateNotificationGroupCommandValidator : AbstractValidator<UpdateNotificationGroupCommand>
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly INotificationGroupQuery _notificationGroupQuery;
        public UpdateNotificationGroupCommandValidator(MaxLengthProvider maxLengthProvider, INotificationGroupQuery notificationGroupQuery)
        {
            var maxLength = maxLengthProvider.GetMaxLength<Domain.Entities.Notification.NotificationGroup>("GroupName") ?? 250;
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
                        RuleFor(x => x.GroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationGroupCommand.GroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateNotificationGroupCommand.GroupName)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => new { x.GroupName, x.Id })
                         .MustAsync(async (notification, cancellation) =>
                      !await _notificationGroupQuery.AlreadyExistsAsync(notification.GroupName, notification.Id))
                         .WithName("Group Name")
                          .WithMessage($"{rule.Error}");
                        break;
                        
                    case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _notificationGroupQuery.NotFoundAsync(Id))             
                           .WithName("Notification Group Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                }
            }
        }
    }
}