using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyById
{
    public class GetAssetWarrantyByIdQuery : IRequest<AssetWarrantyDTO>
    {
         public int Id { get; set; }
    }
}