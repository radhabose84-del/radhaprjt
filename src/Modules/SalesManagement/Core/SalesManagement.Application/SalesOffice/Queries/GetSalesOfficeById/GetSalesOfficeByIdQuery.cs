using MediatR;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById
{
    public class GetSalesOfficeByIdQuery : IRequest<SalesOfficeDto?>
    {
        public int Id { get; set; }
    }
}
