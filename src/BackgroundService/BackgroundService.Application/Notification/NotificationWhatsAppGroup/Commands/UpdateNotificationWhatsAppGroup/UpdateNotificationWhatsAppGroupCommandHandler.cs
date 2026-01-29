using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup
{
    public class UpdateNotificationWhatsAppGroupCommandHandler
        : IRequestHandler<UpdateNotificationWhatsAppGroupCommand, bool>
    {
        private readonly INotificationWhatsAppGroupCommand _command;
        private readonly IMapper _mapper;

        public UpdateNotificationWhatsAppGroupCommandHandler(
            INotificationWhatsAppGroupCommand command,
            IMapper mapper)
        {
            _command = command;
            _mapper  = mapper;
        }

        public async Task<bool> Handle(
            UpdateNotificationWhatsAppGroupCommand request,
            CancellationToken cancellationToken)
        {
            var trimmedName = request.GroupName.Trim();

            var exists = await _command.ExistsByNameAsync(
                trimmedName,
                request.DepartmentId,
                excludeId: request.Id,
                ct: cancellationToken);

            if (exists)
            {
                throw new ExceptionRules("Group Name already exists for this Unit and Department.");
            }

            var entity = _mapper.Map<NotificationWhatsAppGroupEntity>(request);
            entity.GroupName = trimmedName;

            var updated = await _command.UpdateAsync(entity);

            if (!updated)
            {
                throw new ExceptionRules("Notification WhatsApp Group update failed.");
            }

            return true;
        }
    }
}
