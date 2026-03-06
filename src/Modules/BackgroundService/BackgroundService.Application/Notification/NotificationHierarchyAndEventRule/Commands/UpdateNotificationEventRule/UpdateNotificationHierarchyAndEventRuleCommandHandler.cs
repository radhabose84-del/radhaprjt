using AutoMapper;
using MediatR;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Common;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
{
    public class UpdateNotificationHierarchyAndEventRuleCommandHandler
        : IRequestHandler<UpdateNotificationHierarchyAndEventRuleCommand, bool>
    {
        private readonly INotificationLevelHierarchyCommand _levelHierarchyCommand;
        private readonly INotificationEventRuleCommand _eventRuleCommand;
        private readonly IMapper _mapper;

        public UpdateNotificationHierarchyAndEventRuleCommandHandler(
            INotificationLevelHierarchyCommand levelHierarchyCommand,
            INotificationEventRuleCommand eventRuleCommand,
            IMapper mapper)
        {
            _levelHierarchyCommand = levelHierarchyCommand;
            _eventRuleCommand = eventRuleCommand;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateNotificationHierarchyAndEventRuleCommand request, CancellationToken cancellationToken)
        {
            var existing = await _levelHierarchyCommand.GetByIdWithEventRuleAsync(request.NotificationLevelHierarchyId);

            if (existing == null)
                throw new ExceptionRules("Notification Level Hierarchy not found.");

            // 🔹 Step 1: Delete old event rules
            if (existing.NotificationEventRules != null && existing.NotificationEventRules.Any())
            {
                var deleteResult = await _eventRuleCommand.DeleteRangeAsync(existing.NotificationEventRules.ToList());
                if (!deleteResult)
                    throw new ExceptionRules("Failed to delete existing event rules.");
            }

            // 🔹 Step 2: Update parent entity
            existing.NotificationConfigId = request.NotificationConfigId;
            existing.TargetTypeId = request.TargetTypeId;
            existing.TargetId = request.TargetId;
            existing.ApprovalModeId = request.ApprovalModeId;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive == 1
                    ? BaseEntity.Status.Active
                    : BaseEntity.Status.Inactive;

            // 🔹 Step 3: Map and assign new event rules
            existing.NotificationEventRules = request.NotificationEventRules
                .Select(dto => _mapper.Map<NotificationEventRule>(dto))
                .ToList();

            // 🔹 Step 4: Save
            var result = await _levelHierarchyCommand.UpdateAsync(existing);

            if (!result)
                throw new ExceptionRules("Failed to update Notification Hierarchy and Event Rules.");

            return true;
        }
    }
}
