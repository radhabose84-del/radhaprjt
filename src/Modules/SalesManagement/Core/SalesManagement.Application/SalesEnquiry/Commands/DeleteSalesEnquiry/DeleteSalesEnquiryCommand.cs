using MediatR;

namespace SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry
{
    public sealed record DeleteSalesEnquiryCommand(int Id) : IRequest<bool>;
}
