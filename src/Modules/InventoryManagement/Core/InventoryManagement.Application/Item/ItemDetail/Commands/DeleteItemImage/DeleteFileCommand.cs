using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.DeleteItemImage
{
    public class DeleteFileCommand : IRequest<bool>
    {        public string? imagePath { get; set; }       
    }
}