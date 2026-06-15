using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn
{
    public class CreateSalesReturnCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public DateOnly ReturnDate { get; set; }
        public int ComplaintHeaderId { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateSalesReturnInvoiceDto>? InvoiceDetails { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
