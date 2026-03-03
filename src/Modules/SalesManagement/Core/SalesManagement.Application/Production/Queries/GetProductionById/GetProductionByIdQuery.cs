using MediatR;
using SalesManagement.Application.Production.Dto;

namespace SalesManagement.Application.Production.Queries.GetProductionById
{
    public class GetProductionByIdQuery : IRequest<ProductionPackHeaderDto?>
    {
        public int Id { get; set; }
    }
}
