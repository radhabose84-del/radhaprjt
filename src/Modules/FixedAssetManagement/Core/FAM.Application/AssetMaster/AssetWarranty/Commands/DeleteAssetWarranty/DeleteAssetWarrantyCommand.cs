using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty
{
    public class DeleteAssetWarrantyCommand :  IRequest<AssetWarrantyDTO>
    {
         public int Id { get; set; }    
    }
}