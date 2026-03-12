using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.CreateProcurementType
{
    public class CreateProcurementTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? ProcurementName { get; set; }
    }
}
