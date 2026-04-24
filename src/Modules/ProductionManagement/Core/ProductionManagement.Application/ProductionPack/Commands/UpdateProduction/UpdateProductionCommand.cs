using Contracts.Common;
using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Commands.UpdateProduction
{
    public class UpdateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public UpdateProductionDto? ProductionPackEntries { get; set; }
    }
}
