using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById
{
    public class GetShiftMasterByIdQuery : IRequest<ApiResponseDTO<ShiftMasterDetailByIdDto>>
    {
        public int Id { get; set; }
    }
}