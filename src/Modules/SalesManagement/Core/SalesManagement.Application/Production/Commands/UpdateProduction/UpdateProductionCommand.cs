using Contracts.Common;
using MediatR;
using SalesManagement.Application.Production.Dto;

namespace SalesManagement.Application.Production.Commands.UpdateProduction
{
    public class UpdateProductionCommand : IRequest<ApiResponseDTO<int>>
    {
        public UpdateProductionDto? ProductionPackDetails { get; set; }
    }
}
