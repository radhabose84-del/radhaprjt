using MediatR;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitById
{
    public class GetCustomerVisitByIdQuery : IRequest<CustomerVisitDto?>
    {
        public int Id { get; set; }
    }
}
