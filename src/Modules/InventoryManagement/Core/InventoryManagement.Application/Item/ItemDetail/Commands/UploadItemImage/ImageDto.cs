
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.UploadItemImage
{
    public class ImageDto : IMapFrom<ItemMaster>
    {
        public string? Image { get; set; }
        public string? ImageBase64 { get; set; } 

    }
}