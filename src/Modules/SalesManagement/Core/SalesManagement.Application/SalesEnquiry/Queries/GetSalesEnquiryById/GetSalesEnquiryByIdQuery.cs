using MediatR;
using SalesManagement.Application.SalesEnquiry.Dto;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryById
{
    public class GetSalesEnquiryByIdQuery : IRequest<SalesEnquiryHeaderDto?>
    {
        public int Id { get; set; }
    }
}
