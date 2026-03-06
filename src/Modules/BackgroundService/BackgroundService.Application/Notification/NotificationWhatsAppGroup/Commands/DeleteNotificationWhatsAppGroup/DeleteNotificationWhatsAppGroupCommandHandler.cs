using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using MediatR;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup
{
    public class DeleteNotificationWhatsAppGroupCommandHandler 
        : IRequestHandler<DeleteNotificationWhatsAppGroupCommand, bool>
    {
        private readonly INotificationWhatsAppGroupCommand _repo;
        private readonly IMapper _mapper;

        public DeleteNotificationWhatsAppGroupCommandHandler(
            INotificationWhatsAppGroupCommand repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<bool> Handle(DeleteNotificationWhatsAppGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = new NotificationWhatsAppGroupEntity
            {
                IsDeleted = IsDelete.Deleted
            };

            var result = await _repo.DeleteAsync(request.Id, entity);

            if (!result)
                throw new ExceptionRules("Delete failed. Record not found.");

            return true;
        }
    }
}
