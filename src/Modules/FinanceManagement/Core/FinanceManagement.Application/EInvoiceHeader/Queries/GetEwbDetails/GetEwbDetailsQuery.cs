using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEwbDetails
{
    public class GetEwbDetailsQuery : IRequest<ApiResponseDTO<object>>
    {
        public int EInvoiceHeaderId { get; set; }
    }
}
