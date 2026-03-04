using Contracts.Common;
using MediatR;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Application.ProductionPack.Commands.UpdateProduction
{
    public class UpdateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public UpdateProductionDto? ProductionPackDetails { get; set; }
    }
}
