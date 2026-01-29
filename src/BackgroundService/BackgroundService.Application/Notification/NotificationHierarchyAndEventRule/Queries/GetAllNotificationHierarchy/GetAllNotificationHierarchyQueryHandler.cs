using AutoMapper;
using MediatR;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetAllNotificationHierarchy;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById

{
    public class GetAllNotificationHierarchyQueryHandler 
        : IRequestHandler<GetAllNotificationHierarchyQuery, ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>>
    {
        private readonly INotificationLevelHierarchyCommand _repository;
        private readonly IMapper _mapper;

        public GetAllNotificationHierarchyQueryHandler(INotificationLevelHierarchyCommand repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>> Handle(
            GetAllNotificationHierarchyQuery request,
            CancellationToken cancellationToken)
        {
            var (entities, totalCount) = await _repository.GetAllWithEventRuleAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtoList = _mapper.Map<List<NotificationHierarchyAndEventRuleDto>>(entities);
            
            return new ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>
            {
                Data = dtoList,
                TotalCount = totalCount,
                IsSuccess = true,
                Message = "Success",
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
