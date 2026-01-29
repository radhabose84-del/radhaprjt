    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
    using BackgroundService.Application.Notification.Exceptions;
    using BackgroundService.Domain.Entities.Notification;
    using MediatR;

    namespace BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember
    {
        public class UpdateNotificationGroupMemberCommandHandler : IRequestHandler<UpdateNotificationGroupMemberCommand, bool>
        {
            private readonly INotificationGroupMemberCommand _notificationGroupMemberCommand;
            private readonly IMapper _mapper;
            public UpdateNotificationGroupMemberCommandHandler(INotificationGroupMemberCommand notificationGroupMemberCommand,  IMapper mapper)
            {
                _notificationGroupMemberCommand = notificationGroupMemberCommand;
                _mapper = mapper;   
            }
            public async Task<bool> Handle(UpdateNotificationGroupMemberCommand request, CancellationToken cancellationToken)
            {
                var result = await _notificationGroupMemberCommand.UpdateMultipleAsync(request.GroupId, request.UserIds, request.IsActive);
                return result ? result : throw new ExceptionRules("Notification Group Member update failed.");
            }
        }
    }