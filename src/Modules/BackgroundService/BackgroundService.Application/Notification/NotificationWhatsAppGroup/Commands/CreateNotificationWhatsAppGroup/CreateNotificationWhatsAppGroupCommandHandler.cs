using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup
{
    public class CreateNotificationWhatsAppGroupCommandHandler
        : IRequestHandler<CreateNotificationWhatsAppGroupCommand, int>
    {
        private readonly INotificationWhatsAppGroupCommand _command;
        private readonly IMapper _mapper;

        public CreateNotificationWhatsAppGroupCommandHandler(
            INotificationWhatsAppGroupCommand command,
            IMapper mapper)
        {
            _command = command;
            _mapper  = mapper;
        }

        public async Task<int> Handle(
            CreateNotificationWhatsAppGroupCommand request,
            CancellationToken cancellationToken)
        {
            var trimmedName = request.GroupName.Trim();

            var exists = await _command.ExistsByNameAsync(
                trimmedName,
                request.DepartmentId,
                excludeId: null,
                ct: cancellationToken);

            if (exists)
            {
                throw new ExceptionRules("Group Name already exists for this Unit and Department.");
            }

            var entity = _mapper.Map<NotificationWhatsAppGroupEntity>(request);
            entity.GroupName = trimmedName;

            var id = await _command.CreateAsync(entity);

            if (id <= 0)
                throw new ExceptionRules("Notification WhatsApp Group creation failed.");

            return id;
        }
    }
}
