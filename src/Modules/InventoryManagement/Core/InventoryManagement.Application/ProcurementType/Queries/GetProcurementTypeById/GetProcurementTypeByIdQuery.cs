using InventoryManagement.Application.ProcurementType.Dto;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeById
{
    public class GetProcurementTypeByIdQuery : IRequest<ProcurementTypeDto?>
    {
        public int Id { get; set; }
    }
}
