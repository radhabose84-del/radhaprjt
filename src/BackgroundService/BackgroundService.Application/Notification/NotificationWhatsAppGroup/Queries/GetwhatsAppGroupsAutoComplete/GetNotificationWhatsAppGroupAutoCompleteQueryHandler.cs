using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using Contracts.Interfaces.External.IUser; //IDepartmentGrpcClient, IUnitGrpcClient
using Grpc.Core;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete
{
    public class GetNotificationWhatsAppGroupAutoCompleteQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupAutoCompleteQuery, List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;

        public GetNotificationWhatsAppGroupAutoCompleteQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            IDepartmentGrpcClient departmentGrpcClient)
        {
            _queryRepo = queryRepo;
            _departmentGrpcClient = departmentGrpcClient;
        }

        public async Task<List<NotificationWhatsAppGroupAutoCompleteDto>> Handle(
            GetNotificationWhatsAppGroupAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var list = await _queryRepo.GetAutoCompleteAsync(request.SearchTerm, cancellationToken);

            if (list.Count == 0)
                return list;

            // ✅ Enrich DepartmentName (like your GetAll)
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
            catch (RpcException)
            {
                // ignore - return without names
            }

            return list;
        }
    }
}
