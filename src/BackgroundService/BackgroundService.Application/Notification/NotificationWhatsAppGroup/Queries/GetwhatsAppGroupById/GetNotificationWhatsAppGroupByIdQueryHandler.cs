using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using Contracts.Interfaces.External.IUser;
using Grpc.Core;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById
{
    public class GetNotificationWhatsAppGroupByIdQueryHandler
        : IRequestHandler<GetNotificationWhatsAppGroupByIdQuery, NotificationWhatsAppGroupDto?>
    {
        private readonly INotificationWhatsAppGroupQuery _queryRepo;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;
        private readonly IUnitGrpcClient _unitGrpcClient;

        public GetNotificationWhatsAppGroupByIdQueryHandler(
            INotificationWhatsAppGroupQuery queryRepo,
            IDepartmentGrpcClient departmentGrpcClient,
            IUnitGrpcClient unitGrpcClient)
        {
            _queryRepo = queryRepo;
            _departmentGrpcClient = departmentGrpcClient;
            _unitGrpcClient = unitGrpcClient;
        }

        public async Task<NotificationWhatsAppGroupDto?> Handle(
            GetNotificationWhatsAppGroupByIdQuery request,
            CancellationToken cancellationToken)
        {
            var dto = await _queryRepo.GetByIdAsync(request.Id, cancellationToken);
            if (dto == null)
                return null;

            // ✅ Enrich names (same style as GetAll)
            try
            {
                var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
                var dept = departments.FirstOrDefault(d => d.DepartmentId == dto.DepartmentId);
                if (dept != null)
                    dto.DepartmentName = dept.DepartmentName ?? string.Empty;
            }
            catch (RpcException) { }

            try
            {
                var units = await _unitGrpcClient.GetAllUnitAsync();
                var unit = units.FirstOrDefault(u => u.UnitId == dto.UnitId);
                if (unit != null)
                    dto.UnitName = unit.UnitName ?? string.Empty;
            }
            catch (RpcException) { }

            // ✅ Ensure ApiKey never null
            dto.ApiKey ??= string.Empty;

            return dto;
        }
    }
}
