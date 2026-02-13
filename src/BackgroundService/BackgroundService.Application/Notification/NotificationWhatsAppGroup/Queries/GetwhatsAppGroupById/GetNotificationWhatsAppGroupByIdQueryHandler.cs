using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById
{
    public class GetNotificationWhatsAppGroupByIdQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupByIdQuery, NotificationWhatsAppGroupDto?>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly ILookupRepository _lookupRepository;

        public GetNotificationWhatsAppGroupByIdQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            ILookupRepository lookupRepository)
        {
            _queryRepo = queryRepo;
            _lookupRepository = lookupRepository;
        }

        public async Task<NotificationWhatsAppGroupDto?> Handle(
            GetNotificationWhatsAppGroupByIdQuery request,
            CancellationToken cancellationToken)
        {
            var dto = await _queryRepo.GetByIdAsync(request.Id, cancellationToken);
            if (dto == null)
                return null;

            var departments = await _lookupRepository.GetDepartmentsAsync(cancellationToken);
            if (departments.TryGetValue(dto.DepartmentId, out var deptName))
            {
                dto.DepartmentName = deptName;
            }

            var units = await _lookupRepository.GetUnitsAsync(cancellationToken);
            if (units.TryGetValue(dto.UnitId, out var unitName))
            {
                dto.UnitName = unitName;
            }

            // ✅ Ensure ApiKey never null
            dto.ApiKey ??= string.Empty;

            return dto;
        }
    }
}
