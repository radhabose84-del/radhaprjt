using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.ProductionPack.Commands.StockClose
{
    public class StockCloseCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly ClosingDate { get; set; }
    }
}
