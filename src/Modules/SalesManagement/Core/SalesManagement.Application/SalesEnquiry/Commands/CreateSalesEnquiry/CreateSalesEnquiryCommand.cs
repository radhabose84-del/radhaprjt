using MediatR;

namespace SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry
{
    public class CreateSalesEnquiryCommand : IRequest<int>
    {
        public CreateSalesEnquiryDto SalesEnquiryDetails { get; set; } = null!;
    }
}
