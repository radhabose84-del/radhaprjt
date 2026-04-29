using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAmendmentById
{
    public class GetSalesQuotationAmendmentByIdQuery : IRequest<ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>>
    {
        public int SalesQuotationHeaderId { get; set; }
    }
}
