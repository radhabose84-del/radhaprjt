using MediatR;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Application.ProductionPack.Queries.GetProductionById
{
    public class GetProductionByIdQuery : IRequest<ProductionPackHeaderDto?>
    {
        public int Id { get; set; }
    }
}
