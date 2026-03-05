using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationGroupMember
{
    public class UpdateNotificationGroupMemberCommandValidator : AbstractValidator<UpdateNotificationGroupMemberCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationGroupMemberQuery _notificationGroupQuery;

        public UpdateNotificationGroupMemberCommandValidator(INotificationGroupMemberQuery notificationGroupQuery)
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
                        .GreaterThan(0)
                        .WithMessage("GroupId is required and must be greater than zero.");

                        RuleFor(x => x.UserIds)
                            .NotEmpty()
                            .WithMessage("At least one UserId must be provided.")
                            .Must(list => list.Distinct().Count() == list.Count)
                            .WithMessage("Duplicate UserIds are not allowed in the same request.");

                        
                        break;
                  /*   case "AlreadyExists":
                        RuleForEach(x => x.UserIds)
                            .MustAsync(async (command, userId, cancellation) =>
                            !await notificationGroupQuery.AlreadyExistsAsync(command.GroupId, userId))
                            .WithMessage("UserId '{PropertyValue}' already exists in this group.");
                        break; */
                        
                    case "NotFound":
                           RuleFor(x => x.GroupId )
                           .MustAsync(async (GroupId, cancellation) => 
                        await _notificationGroupQuery.NotFoundAsync(GroupId))             
                           .WithName("Notification Group Member Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                }
            }
        }
    }
}