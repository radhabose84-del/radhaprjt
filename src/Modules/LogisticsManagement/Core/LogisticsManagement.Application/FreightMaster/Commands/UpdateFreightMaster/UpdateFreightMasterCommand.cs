using Contracts.Common;
using MediatR;

namespace LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster
{
    public class UpdateFreightMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int FreightModeId { get; set; }
        public int RateMethodId { get; set; }
        public decimal Rate { get; set; }
        public int ModuleId { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
