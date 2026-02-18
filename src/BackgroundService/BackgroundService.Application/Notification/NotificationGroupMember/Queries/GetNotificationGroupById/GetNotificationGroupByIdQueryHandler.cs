using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupById;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupMemberById
{
    public class GetNotificationGroupByIdQueryHandler : IRequestHandler<GetNotificationGroupByIdQuery, ApiResponseDTO<GetNotificationGroupMemberDto>>
    {
        private readonly INotificationGroupMemberQuery _notificationGroupMemberQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetNotificationGroupByIdQueryHandler(INotificationGroupMemberQuery notificationGroupMemberQuery, IMediator mediator, IMapper mapper)
        {
            _notificationGroupMemberQuery = notificationGroupMemberQuery;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<GetNotificationGroupMemberDto>> Handle(GetNotificationGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var notificationGroup = await _notificationGroupMemberQuery.GetByIdAsync(request.Id);

            if (notificationGroup == null)
            {
                return new ApiResponseDTO<GetNotificationGroupMemberDto>
                {
                    IsSuccess = false,
                    Message = $"Notification Group with Id {request.Id} not found.",
                    Data = null
                };
            }

            // If AutoMapper is needed (optional, since repository already returns NotificationGroupDto)
            var notificationGroupDto = _mapper.Map<GetNotificationGroupMemberDto>(notificationGroup);

            return new ApiResponseDTO<GetNotificationGroupMemberDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = notificationGroupDto
            };
        }
    }
}
