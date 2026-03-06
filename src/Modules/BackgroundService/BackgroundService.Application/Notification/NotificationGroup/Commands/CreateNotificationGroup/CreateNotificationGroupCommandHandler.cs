using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup
{
    public class CreateNotificationGroupCommandHandler : IRequestHandler<CreateNotificationGroupCommand, int>
    {
        private readonly INotificationGroupCommand _notificationGroupCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateNotificationGroupCommandHandler(INotificationGroupCommand notificationGroupCommand, IMediator imediator, IMapper imapper)
        {
            _notificationGroupCommand = notificationGroupCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<int> Handle(CreateNotificationGroupCommand request, CancellationToken cancellationToken)
        {
            var NotificationGroup = _imapper.Map<Domain.Entities.Notification.NotificationGroup>(request);
            
            var result = await _notificationGroupCommand.CreateAsync(NotificationGroup);
            
            return result > 0 ? result : throw new ExceptionRules("NotificationGroup Creation Failed.");
        }
    }
}