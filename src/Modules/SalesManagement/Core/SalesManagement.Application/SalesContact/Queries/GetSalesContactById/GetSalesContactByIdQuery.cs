using MediatR;
using SalesManagement.Application.SalesContact.Dto;

namespace SalesManagement.Application.SalesContact.Queries.GetSalesContactById
{
    public class GetSalesContactByIdQuery : IRequest<SalesContactDto?>
    {
        public int Id { get; set; }
    }
}
