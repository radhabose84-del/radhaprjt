using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteFileAssetWarranty
{
    public class DeleteFileAssetWarrantyCommand : IRequest<bool>
    {
        public string? assetPath { get; set; }
    }
}