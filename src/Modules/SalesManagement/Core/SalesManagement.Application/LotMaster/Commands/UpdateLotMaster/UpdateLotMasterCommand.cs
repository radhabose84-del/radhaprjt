using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.LotMaster.Commands.UpdateLotMaster
{
    public class UpdateLotMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int LotTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public int StatusId { get; set; }
        public string? ProductionOrderRef { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
