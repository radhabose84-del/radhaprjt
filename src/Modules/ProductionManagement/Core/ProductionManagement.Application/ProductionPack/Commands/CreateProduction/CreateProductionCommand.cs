using Contracts.Common;
using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Commands.CreateProduction
{
    public class CreateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public CreateProductionDto? ProductionPackEntries { get; set; }
    }
}
