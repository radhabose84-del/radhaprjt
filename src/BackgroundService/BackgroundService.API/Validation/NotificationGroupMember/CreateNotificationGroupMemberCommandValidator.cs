using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using FluentValidation;

namespace BackgroundService.API.Validation.NotificationGroupMember
{
    public class CreateNotificationGroupMemberCommandValidator : AbstractValidator<CreateNotificationGroupMemberCommand>
    {
         private readonly List<Common.ValidationRule> _validationRules;
        private readonly INotificationGroupMemberQuery _notificationGroupQuery;
        public CreateNotificationGroupMemberCommandValidator(INotificationGroupMemberQuery notificationGroupQuery)
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
                        RuleFor(x => x.GroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationGroupMemberCommand.GroupId)} {rule.Error}");

                            RuleFor(x => x.UserIds)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateNotificationGroupMemberCommand.UserIds)} {rule.Error}");

                            RuleFor(x => x.UserIds)
                                .Must(list => list.Distinct().Count() == list.Count)
                                .WithMessage("Duplicate UserIds are not allowed in the same request.");
                        break;
                      case "AlreadyExists":                          
                        RuleForEach(x => x.UserIds)
                            .MustAsync(async (command, userId, cancellation) =>
                                !await _notificationGroupQuery.AlreadyExistsAsync(command.GroupId, userId))
                            .WithMessage("UserId '{PropertyValue}' already exists in this group.");                        
                        break;
                }
            }
        }
    }
}