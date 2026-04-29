using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendmentById
{
    public class GetPendingSalesQuotationAmendmentByIdQuery
        : IRequest<SalesQuotationAmendmentHeaderDto?>
    {
        public int Id { get; set; }
    }
}
