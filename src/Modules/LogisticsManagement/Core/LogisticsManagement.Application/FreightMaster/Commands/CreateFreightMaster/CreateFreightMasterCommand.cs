using Contracts.Common;
using MediatR;

namespace LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster
{
    public class CreateFreightMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int FreightModeId { get; set; }
        public int RateMethodId { get; set; }
        public decimal Rate { get; set; }
        public int ModuleId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
