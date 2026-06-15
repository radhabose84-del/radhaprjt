using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.ServiceMaster.Commands.UpdateService
{
    public class UpdateServiceCommand  : IRequest<GetServiceMasterDto>, IRequirePermission
    {
        public int Id { get; set; }

        public string ServiceDescription { get; set; } = default!;
        public int SacId { get; set; }
        public int UomId { get; set; }
        public int ServiceCategoryId { get; set; }

        // optional: change status
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
