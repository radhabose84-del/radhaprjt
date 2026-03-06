using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Queries.GetAllNotificationGroup
{
    public class GetAllNotificationGroupQueryHandler : IRequestHandler<GetAllNotificationGroupQuery, ApiResponseDTO<List<NotificationGroupDto>>>
    {
         private readonly INotificationGroupQuery _notificationGroupQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetAllNotificationGroupQueryHandler( INotificationGroupQuery notificationGroupQuery, IMediator mediator, IMapper mapper)
        {
            _notificationGroupQuery = notificationGroupQuery;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<ApiResponseDTO<List<NotificationGroupDto>>> Handle(GetAllNotificationGroupQuery request, CancellationToken cancellationToken)
        {
            var (NotificationGroup, TotalCount) = await _notificationGroupQuery.GetAllNotificationGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var NotificationGroupDto = _mapper.Map<List<NotificationGroupDto>>(NotificationGroup);


            return new ApiResponseDTO<List<NotificationGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = NotificationGroupDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}