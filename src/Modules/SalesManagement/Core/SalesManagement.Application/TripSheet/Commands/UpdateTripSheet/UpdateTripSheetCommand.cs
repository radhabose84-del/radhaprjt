using Contracts.Common;
using MediatR;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;

namespace SalesManagement.Application.TripSheet.Commands.UpdateTripSheet
{
    public class UpdateTripSheetCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public DateOnly TripDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<CreateTripSheetDetailItem>? Details { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
