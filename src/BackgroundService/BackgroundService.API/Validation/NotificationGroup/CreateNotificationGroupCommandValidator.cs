using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup;
using FluentValidation;

namespace BackgroundService.API.Validation.NotificationGroup
{
    public class CreateNotificationGroupCommandValidator : AbstractValidator<CreateNotificationGroupCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly INotificationGroupQuery _notificationGroupQuery;
        public CreateNotificationGroupCommandValidator(MaxLengthProvider maxLengthProvider, INotificationGroupQuery notificationGroupQuery)
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
                            .WithMessage($"{nameof(CreateNotificationGroupCommand.GroupName)} {rule.Error}");
                        break;
                    case "MaxLength":                        
                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateNotificationGroupCommand.GroupName)} {rule.Error}");
                        break;
                    case "AlreadyExists":                       
                        RuleFor(x => x.GroupName)
                          .MustAsync(async (GroupName, cancellation) => !await _notificationGroupQuery.AlreadyExistsAsync(GroupName))
                           .WithName("Group Name")
                             .WithMessage($"{rule.Error}");
                        break;

                }
            }
        }
    }
}