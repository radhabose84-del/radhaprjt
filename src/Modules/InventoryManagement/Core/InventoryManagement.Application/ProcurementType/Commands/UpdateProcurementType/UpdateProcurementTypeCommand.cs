using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.UpdateProcurementType
{
    public class UpdateProcurementTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? ProcurementName { get; set; }
        public int IsActive { get; set; }
    }
}
