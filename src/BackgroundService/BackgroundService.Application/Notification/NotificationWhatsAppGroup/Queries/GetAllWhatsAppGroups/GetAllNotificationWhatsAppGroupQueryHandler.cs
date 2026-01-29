using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using Contracts.Interfaces.External.IUser; // IDepartmentGrpcClient, IUnitGrpcClient
using Grpc.Core;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup
{
    public class GetAllNotificationWhatsAppGroupQueryHandler
        : IRequestHandler<GetAllNotificationWhatsAppGroupQuery,
            (List<NotificationWhatsAppGroupDto> Items, int TotalCount, int PageNumber, int PageSize)>
    {
        private readonly INotificationWhatsAppGroupQuery _notificationWhatsAppGroupQuery;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;   // ✅ change here
        private readonly IUnitGrpcClient _unitGrpcClient;

        public GetAllNotificationWhatsAppGroupQueryHandler(
            INotificationWhatsAppGroupQuery notificationWhatsAppGroupQuery,
            IDepartmentGrpcClient departmentGrpcClient,                 // ✅ change here
            IUnitGrpcClient unitGrpcClient)
        {
            _notificationWhatsAppGroupQuery = notificationWhatsAppGroupQuery;
            _departmentGrpcClient = departmentGrpcClient;
            _unitGrpcClient = unitGrpcClient;
        }

        public async Task<(List<NotificationWhatsAppGroupDto> Items, int TotalCount, int PageNumber, int PageSize)> Handle(
            GetAllNotificationWhatsAppGroupQuery request,
            CancellationToken cancellationToken)
        {
            var filter = new NotificationWhatsAppGroupListFilterDto
            {
                PageNumber   = request.PageNumber,
                PageSize     = request.PageSize,
                SearchTerm   = request.SearchTerm,
                DepartmentId = request.DepartmentId
            };

            var (items, totalCount) = await _notificationWhatsAppGroupQuery
                .GetAllAsync(filter, cancellationToken);

            if (items.Count > 0)
            {
                // ✅ Department Name
                try
                {
                    var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
                    var deptLookup = departments
                        .GroupBy(d => d.DepartmentId)
                        .ToDictionary(g => g.Key, g => g.First().DepartmentName);

                    foreach (var dto in items)
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
                    // ignore grpc failure, return without names
                }

                // ✅ Unit Name
                try
                {
                    var units = await _unitGrpcClient.GetAllUnitAsync(); // whatever your method returns
                    var unitLookup = units
                        .GroupBy(u => u.UnitId)
                        .ToDictionary(g => g.Key, g => g.First().UnitName);

                    foreach (var dto in items)
                    {
                        if (string.IsNullOrWhiteSpace(dto.UnitName) &&
                            unitLookup.TryGetValue(dto.UnitId, out var unitName))
                        {
                            dto.UnitName = unitName;
                        }
                    }
                }
                catch (RpcException)
                {
                    // ignore grpc failure, return without names
                }
            }

            return (items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
