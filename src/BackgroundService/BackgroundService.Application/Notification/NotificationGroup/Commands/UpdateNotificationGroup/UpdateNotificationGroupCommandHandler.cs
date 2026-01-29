using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup
{
    public class UpdateNotificationGroupCommandHandler : IRequestHandler<UpdateNotificationGroupCommand, bool>
    {
        private readonly INotificationGroupCommand _notificationGroupCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateNotificationGroupCommandHandler(INotificationGroupCommand notificationGroupCommand, IMediator imediator, IMapper imapper)
        {
            _notificationGroupCommand = notificationGroupCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(UpdateNotificationGroupCommand request, CancellationToken cancellationToken)
        {
            
            var NotificationGroup = _imapper.Map<Domain.Entities.Notification.NotificationGroup>(request);
            var result = await _notificationGroupCommand.UpdateAsync(NotificationGroup);
            
           
            return result == true ? result : throw new ExceptionRules("Notification Group update failed.");   
        }
    }
}