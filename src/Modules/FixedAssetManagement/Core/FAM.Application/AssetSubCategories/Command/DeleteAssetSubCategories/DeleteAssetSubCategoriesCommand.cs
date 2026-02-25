using MediatR;

namespace FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories
{
    public class DeleteAssetSubCategoriesCommand :IRequest<int>
    {
         public int Id { get; set; }
    }
}