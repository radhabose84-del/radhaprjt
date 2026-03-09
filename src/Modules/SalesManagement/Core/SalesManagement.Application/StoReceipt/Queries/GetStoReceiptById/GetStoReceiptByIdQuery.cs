using MediatR;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.StoReceipt.Queries.GetStoReceiptById
{
    public class GetStoReceiptByIdQuery : IRequest<StoReceiptHeaderDto?>
    {
        public int Id { get; set; }
    }
}
