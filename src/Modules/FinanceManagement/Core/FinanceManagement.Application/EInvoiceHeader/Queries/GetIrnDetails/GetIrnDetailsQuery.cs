using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetIrnDetails
{
    public class GetIrnDetailsQuery : IRequest<ApiResponseDTO<object>>
    {
        public int EInvoiceHeaderId { get; set; }
    }
}
