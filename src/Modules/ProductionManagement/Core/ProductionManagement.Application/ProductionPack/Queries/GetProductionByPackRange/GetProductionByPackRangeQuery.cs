using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionByPackRange
{
    public class GetProductionByPackRangeQuery : IRequest<List<ProductionPackDetailDto>>
    {
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }
}
