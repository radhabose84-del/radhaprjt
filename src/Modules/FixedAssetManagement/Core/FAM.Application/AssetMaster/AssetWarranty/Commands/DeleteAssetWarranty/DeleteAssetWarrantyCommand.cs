using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty
{
    public class DeleteAssetWarrantyCommand :  IRequest<AssetWarrantyDTO>
    {
         public int Id { get; set; }    
    }
}