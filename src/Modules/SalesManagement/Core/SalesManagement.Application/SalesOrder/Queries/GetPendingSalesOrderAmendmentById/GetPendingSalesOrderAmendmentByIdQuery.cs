using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendmentById
{
    public class GetPendingSalesOrderAmendmentByIdQuery
        : IRequest<SalesOrderAmendmentHeaderDto?>
    {
        public int Id { get; set; }
    }
}
