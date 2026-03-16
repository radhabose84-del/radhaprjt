using InventoryManagement.Application.UsageType.Dto;
using MediatR;

namespace InventoryManagement.Application.UsageType.Queries.GetUsageTypeById
{
    public class GetUsageTypeByIdQuery : IRequest<UsageTypeDto?>
    {
        public int Id { get; set; }
    }
}
