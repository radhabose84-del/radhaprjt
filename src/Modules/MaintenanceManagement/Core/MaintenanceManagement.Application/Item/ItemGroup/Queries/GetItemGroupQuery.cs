using MediatR;

namespace MaintenanceManagement.Application.Item.ItemGroup.Queries
{
    public class GetItemGroupQuery : IRequest<List<GetItemGroupDto>>
    {
        public string? OldUnitId { get; set; } 
    }
}