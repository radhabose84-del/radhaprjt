using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup
{
    public class UpdateItemGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? ItemGroupCode { get; set; }
        public string? ItemGroupName { get; set; }      
        public byte IsActive { get; set; }
    }
}