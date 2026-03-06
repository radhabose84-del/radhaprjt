using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete
{
    public class GetNotificationWhatsAppGroupAutoCompleteQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupAutoCompleteQuery, List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly ILookupRepository _lookupRepository;

        public GetNotificationWhatsAppGroupAutoCompleteQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            ILookupRepository lookupRepository)
        {
            _queryRepo = queryRepo;
            _lookupRepository = lookupRepository;
        }

        public async Task<List<NotificationWhatsAppGroupAutoCompleteDto>> Handle(
            GetNotificationWhatsAppGroupAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _queryRepo.GetAutoCompleteAsync(request.SearchTerm, cancellationToken);

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
