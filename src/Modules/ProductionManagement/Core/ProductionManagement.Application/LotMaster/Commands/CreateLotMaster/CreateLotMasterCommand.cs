using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.LotMaster.Commands.CreateLotMaster
{
    public class CreateLotMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }
        public int LotTypeId { get; set; }
        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public DateOnly StartDate { get; set; }
        public int StatusId { get; set; }
        public string? ProductionOrderRef { get; set; }
        public string? Remarks { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
