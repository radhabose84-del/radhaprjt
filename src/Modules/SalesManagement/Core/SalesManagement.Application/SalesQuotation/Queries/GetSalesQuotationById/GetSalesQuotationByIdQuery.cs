using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationById
{
    public class GetSalesQuotationByIdQuery : IRequest<SalesQuotationHeaderDto?>
    {
        public int Id { get; set; }
    }
}
