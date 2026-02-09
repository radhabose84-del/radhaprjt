
using BudgetManagement.Application.Common.Mappings;

namespace BudgetManagement.Application.BudgetRequest.Commands.UploadImage
{
    public class ImageDto : IMapFrom<BudgetManagement.Domain.Entities.BudgetRequest>
    {
        public string? Image { get; set; }
        public string? ImageBase64 { get; set; } 

    }
}