using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyById
{
    public class GetAssetWarrantyByIdQuery : IRequest<AssetWarrantyDTO>
    {
         public int Id { get; set; }
    }
}