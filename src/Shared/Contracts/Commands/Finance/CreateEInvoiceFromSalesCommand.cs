using Contracts.Common;
using Contracts.Dtos.Finance;
using MediatR;

namespace Contracts.Commands.Finance
{
    public class CreateEInvoiceFromSalesCommand : IRequest<ApiResponseDTO<EInvoiceCreationResultDto>>
    {
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public int UnitId { get; set; }
        public int SalesOrderType { get; set; }
        public bool IsEwaybillCreate { get; set; }
    }
}
