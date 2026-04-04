using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Commands.UpdateSalesReturn
{
    public class UpdateSalesReturnCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<CreateSalesReturnDetailDto>? Details { get; set; }
    }
}
