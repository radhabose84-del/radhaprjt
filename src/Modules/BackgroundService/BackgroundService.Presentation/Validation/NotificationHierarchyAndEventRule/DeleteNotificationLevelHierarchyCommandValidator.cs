using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule
{
    public class DeleteNotificationLevelHierarchyCommandValidator : AbstractValidator<DeleteNotificationLevelHierarchyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationLevelHierarchyCommand _queryRepository;

        public DeleteNotificationLevelHierarchyCommandValidator(INotificationLevelHierarchyCommand queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteNotificationLevelHierarchyCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                       RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                        .WithMessage("Id not found.");
                        break;

                  /*   case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                !await _queryRepository.SoftDeleteValidation(id))
                            .WithMessage($"{nameof(DeleteNotificationLevelHierarchyCommand.Id)} {rule.Error}");
                        break; */
                }
            }
        }
    }
}
