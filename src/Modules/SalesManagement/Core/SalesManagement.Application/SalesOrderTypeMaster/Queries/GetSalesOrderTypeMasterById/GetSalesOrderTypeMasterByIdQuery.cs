using MediatR;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterById
{
    public class GetSalesOrderTypeMasterByIdQuery : IRequest<SalesOrderTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
