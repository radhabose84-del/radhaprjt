using MediatR;
using SalesManagement.Application.SalesLead.Dto;

namespace SalesManagement.Application.SalesLead.Queries.GetSalesLeadById
{
    public class GetSalesLeadByIdQuery : IRequest<SalesLeadDto?>
    {
        public int Id { get; set; }
    }
}
