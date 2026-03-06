using AutoMapper;
using MediatR;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById
{
    public class GetNotificationHierarchyByIdQueryHandler
        : IRequestHandler<GetNotificationHierarchyByIdQuery, NotificationHierarchyAndEventRuleDto>
    {
        private readonly INotificationLevelHierarchyCommand _queryRepo;
        private readonly IMapper _mapper;

        public GetNotificationHierarchyByIdQueryHandler(
            INotificationLevelHierarchyCommand queryRepo,
            IMapper mapper)
        {
            _queryRepo = queryRepo;
            _mapper = mapper;
        }

        public async Task<NotificationHierarchyAndEventRuleDto> Handle(
            GetNotificationHierarchyByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _queryRepo.GetByIdWithEventRuleAsync(request.Id);

            if (entity == null)
                throw new ExceptionRules("Notification Level Hierarchy not found.");

            return _mapper.Map<NotificationHierarchyAndEventRuleDto>(entity);
        }
    }
}
