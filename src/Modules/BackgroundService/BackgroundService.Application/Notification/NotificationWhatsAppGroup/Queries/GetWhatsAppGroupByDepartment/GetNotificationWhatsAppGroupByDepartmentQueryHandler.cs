using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment
{
    public class GetNotificationWhatsAppGroupByDepartmentQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupByDepartmentQuery, List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly ILookupRepository _lookupRepository;

        public GetNotificationWhatsAppGroupByDepartmentQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            ILookupRepository lookupRepository)
        {
            _queryRepo = queryRepo;
            _lookupRepository = lookupRepository;
        }

        public async Task<List<NotificationWhatsAppGroupAutoCompleteDto>> Handle(
            GetNotificationWhatsAppGroupByDepartmentQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _queryRepo.GetByDepartmentAsync(
                request.DepartmentId,
                request.SearchTerm,
                cancellationToken);

            if (list.Count == 0)
                return list;

            var departments = await _lookupRepository.GetDepartmentsAsync(cancellationToken);
            foreach (var dto in list)
            {
                if (string.IsNullOrWhiteSpace(dto.DepartmentName) &&
                    departments.TryGetValue(dto.DepartmentId, out var deptName))
                {
                    dto.DepartmentName = deptName;
                }
            }

            return list;
        }
    }
}
