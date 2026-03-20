using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionById
{
    public class GetProductionByIdQuery : IRequest<ProductionPackHeaderDto?>
    {
        public int Id { get; set; }
    }
}
