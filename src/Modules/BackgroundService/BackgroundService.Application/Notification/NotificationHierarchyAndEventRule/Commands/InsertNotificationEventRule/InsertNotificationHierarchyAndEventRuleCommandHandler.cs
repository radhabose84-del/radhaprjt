using AutoMapper;
using MediatR;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.InsertNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.InsertNotificationEventRule
{
    public class InsertNotificationHierarchyAndEventRuleCommandHandler 
        : IRequestHandler<InsertNotificationHierarchyAndEventRuleCommand, bool>
    {
        private readonly INotificationLevelHierarchyCommand _levelHierarchyCommand;
        private readonly INotificationEventRuleCommand _eventRuleCommand;
        private readonly IMapper _mapper;

        public InsertNotificationHierarchyAndEventRuleCommandHandler(
            INotificationLevelHierarchyCommand levelHierarchyCommand,
            INotificationEventRuleCommand eventRuleCommand,
            IMapper mapper)
        {
            _levelHierarchyCommand = levelHierarchyCommand;
            _eventRuleCommand = eventRuleCommand;
            _mapper = mapper;
        }

        public async Task<bool> Handle(InsertNotificationHierarchyAndEventRuleCommand request, CancellationToken cancellationToken)
        {
            var hierarchy = _mapper.Map<NotificationLevelHierarchy>(request.Dto);

            // Map and attach all event rules
            hierarchy.NotificationEventRules = request.Dto.NotificationEventRules
                .Select(e => _mapper.Map<NotificationEventRule>(e))
                .ToList();

            var result = await _levelHierarchyCommand.InsertAsync(hierarchy);

            if (!result)
                throw new ExceptionRules("Failed to insert hierarchy with event rules.");

            return true;
        }
    }
}
