using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.DeleteItemImage
{
    public class DeleteFileCommand : IRequest<bool>
    {        public string? imagePath { get; set; }       
    }
}