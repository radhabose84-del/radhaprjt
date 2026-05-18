using MediatR;

namespace FAM.Application.AssetCategories.Command.UpdateAssetCategories
{
    public class UpdateAssetCategoriesCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetGroupId { get; set; }
        public byte IsActive { get; set; }
        //public decimal GroupPercentage { get; set; }       
    }
}