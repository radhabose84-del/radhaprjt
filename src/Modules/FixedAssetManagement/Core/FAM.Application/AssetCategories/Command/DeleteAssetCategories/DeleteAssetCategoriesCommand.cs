using MediatR;

namespace FAM.Application.AssetCategories.Command.DeleteAssetCategories
{
    public class DeleteAssetCategoriesCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}