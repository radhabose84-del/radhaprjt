using MediatR;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Application.Notification.Exceptions;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule
{
    public class DeleteNotificationLevelHierarchyCommandHandler 
        : IRequestHandler<DeleteNotificationLevelHierarchyCommand, bool>
    {
        private readonly INotificationLevelHierarchyCommand _hierarchyRepo;
        private readonly INotificationEventRuleCommand _eventRuleRepo;

        public DeleteNotificationLevelHierarchyCommandHandler(
            INotificationLevelHierarchyCommand hierarchyRepo,
            INotificationEventRuleCommand eventRuleRepo)
        {
            _hierarchyRepo = hierarchyRepo;
            _eventRuleRepo = eventRuleRepo;
        }

        public async Task<bool> Handle(DeleteNotificationLevelHierarchyCommand request, CancellationToken cancellationToken)
        {
            var hierarchy = await _hierarchyRepo.GetByIdAsync(request.Id);
            if (hierarchy == null)
                throw new ExceptionRules("Notification Level Hierarchy not found.");

            // First delete associated event rules
            await _eventRuleRepo.DeleteByHierarchyIdAsync(request.Id);

            // Then delete hierarchy
            var result = await _hierarchyRepo.DeleteAsync(hierarchy);

            if (!result)
                throw new ExceptionRules("Failed to delete Notification Level Hierarchy.");

            return true;
        }
    }
}
