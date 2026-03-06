using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup
{
    public class DeleteNotificationGroupCommandHandler : IRequestHandler<DeleteNotificationGroupCommand, bool>
    {
        private readonly INotificationGroupCommand _notificationGroupCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public DeleteNotificationGroupCommandHandler( INotificationGroupCommand notificationGroupCommand, IMediator imediator, IMapper imapper)
        {
            _notificationGroupCommand = notificationGroupCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(DeleteNotificationGroupCommand request, CancellationToken cancellationToken)
        {
             
            var NotificationGroup = _imapper.Map<Domain.Entities.Notification.NotificationGroup>(request);
            var result = await _notificationGroupCommand.DeleteAsync(request.Id,NotificationGroup);
        
            return result == true ? result : throw new ExceptionRules("Notification Group deletion failed.");
        }
    }
}