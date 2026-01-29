using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using Contracts.Interfaces.External.IUser;
using Grpc.Core;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment
{
    public class GetNotificationWhatsAppGroupByDepartmentQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupByDepartmentQuery, List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;

        public GetNotificationWhatsAppGroupByDepartmentQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            IDepartmentGrpcClient departmentGrpcClient)
        {
            _queryRepo = queryRepo;
            _departmentGrpcClient = departmentGrpcClient;
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

            // ✅ Enrich DepartmentName
            try
            {
                var departments = await _departmentGrpcClient.GetAllDepartmentAsync();

                var deptLookup = departments
                    .GroupBy(d => d.DepartmentId)
                    .ToDictionary(g => g.Key, g => g.First().DepartmentName ?? string.Empty);

                foreach (var dto in list)
                {
                    if (string.IsNullOrWhiteSpace(dto.DepartmentName) &&
                        deptLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    {
                        dto.DepartmentName = deptName;
                    }
                }
            }
            catch (RpcException) { }

            return list;
        }
    }
}
