using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Entities.Notification;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember
{
    public class CreateNotificationGroupMemberCommandHandler : IRequestHandler<CreateNotificationGroupMemberCommand, int>
    {
        private readonly INotificationGroupMemberCommand _notificationGroupMemberCommand;        
        private readonly IMapper _mapper;
        public CreateNotificationGroupMemberCommandHandler(INotificationGroupMemberCommand notificationGroupMemberCommand,  IMapper mapper)
        {
            _notificationGroupMemberCommand = notificationGroupMemberCommand;            
            _mapper = mapper;
        }
        public async Task<int> Handle(CreateNotificationGroupMemberCommand request, CancellationToken cancellationToken)
        {
            if (request.UserIds == null || !request.UserIds.Any())
                throw new ExceptionRules("At least one UserId must be provided.");

            var members = request.UserIds.Select(userId => new NotificationGroupMembers
            {
                GroupId = request.GroupId,
                UserId = userId
            }).ToList();

            var result = await _notificationGroupMemberCommand.CreateMultipleAsync(members);

            return result > 0 ? result : throw new ExceptionRules("Notification Group Member Creation Failed.");
        }
    }
}