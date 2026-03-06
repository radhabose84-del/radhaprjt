using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup
{
    public class GetAllNotificationWhatsAppGroupQueryHandler
        : IRequestHandler<GetAllNotificationWhatsAppGroupQuery,
            (List<NotificationWhatsAppGroupDto> Items, int TotalCount, int PageNumber, int PageSize)>
    {
        private readonly INotificationWhatsAppGroupQuery _notificationWhatsAppGroupQuery;
        private readonly ILookupRepository _lookupRepository;

        public GetAllNotificationWhatsAppGroupQueryHandler(
            INotificationWhatsAppGroupQuery notificationWhatsAppGroupQuery,
            ILookupRepository lookupRepository)
        {
            _notificationWhatsAppGroupQuery = notificationWhatsAppGroupQuery;
            _lookupRepository = lookupRepository;
        }

        public async Task<(List<NotificationWhatsAppGroupDto> Items, int TotalCount, int PageNumber, int PageSize)> Handle(
            GetAllNotificationWhatsAppGroupQuery request,
            CancellationToken cancellationToken)
        {
            var filter = new NotificationWhatsAppGroupListFilterDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SearchTerm = request.SearchTerm,
                DepartmentId = request.DepartmentId
            };

            var (items, totalCount) = await _notificationWhatsAppGroupQuery
                .GetAllAsync(filter, cancellationToken);

            if (items.Count > 0)
            {
                var departments = await _lookupRepository.GetDepartmentsAsync(cancellationToken);
                foreach (var dto in items)
                {
                    if (string.IsNullOrWhiteSpace(dto.DepartmentName) &&
                        departments.TryGetValue(dto.DepartmentId, out var deptName))
                    {
                        dto.DepartmentName = deptName;
                    }
                }

                var units = await _lookupRepository.GetUnitsAsync(cancellationToken);
                foreach (var dto in items)
                {
                    if (string.IsNullOrWhiteSpace(dto.UnitName) &&
                        units.TryGetValue(dto.UnitId, out var unitName))
                    {
                        dto.UnitName = unitName;
                    }

                }
            }

            return (items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
