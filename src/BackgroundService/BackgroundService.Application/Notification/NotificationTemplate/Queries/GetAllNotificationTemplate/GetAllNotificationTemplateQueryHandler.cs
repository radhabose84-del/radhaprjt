using AutoMapper;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate
{
    public class GetAllNotificationTemplateQueryHandler : IRequestHandler<GetAllNotificationTemplateQuery, ApiResponseDTO<List<NotificationTemplateDto>>>
    {
        private readonly INotificationTemplateQueryRepository _NotificationTemplateQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetAllNotificationTemplateQueryHandler(INotificationTemplateQueryRepository NotificationTemplateQueryRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateQueryRepository = NotificationTemplateQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<NotificationTemplateDto>>> Handle(GetAllNotificationTemplateQuery request, CancellationToken cancellationToken)
        {
            var (NotificationTemplate, totalCount) = await _NotificationTemplateQueryRepository.GetAllNotificationTemplateAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var NotificationTemplateDto = _mapper.Map<List<NotificationTemplateDto>>(NotificationTemplate);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetNotificationTemplate",
                actionCode: "Get",
                actionName: NotificationTemplate.Count().ToString(),
                details: "Notification Template details were fetched.",
                module: "NotificationTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<NotificationTemplateDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = NotificationTemplateDto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }



    }
}