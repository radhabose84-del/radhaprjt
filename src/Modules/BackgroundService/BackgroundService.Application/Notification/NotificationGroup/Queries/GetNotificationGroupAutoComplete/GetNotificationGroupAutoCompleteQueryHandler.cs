using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Queries.GetNotificationGroupAutoComplete
{
    public class GetNotificationGroupAutoCompleteQueryHandler : IRequestHandler<GetNotificationGroupAutoCompleteQuery, List<GetNotificationGroupAutoCompleteDto>>
    {
        private readonly INotificationGroupQuery _notificationGroupQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetNotificationGroupAutoCompleteQueryHandler( INotificationGroupQuery notificationGroupQuery, IMediator mediator, IMapper mapper)
        {
            _notificationGroupQuery = notificationGroupQuery;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<List<GetNotificationGroupAutoCompleteDto>> Handle(GetNotificationGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
             var Result = await _notificationGroupQuery.GetNotificationGroupsAutoComplete(request.SearchPattern ?? string.Empty);
            var Notification = _mapper.Map<List<GetNotificationGroupAutoCompleteDto>>(Result);
            
            return Notification;
        }
    }
}