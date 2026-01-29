using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteFileAssetMasterGeneral
{
    public class DeleteFileAssetMasterGeneralCommand : IRequest<bool>
    {
        public string? assetPath { get; set; }
        // public string? CompanyName { get; set; }  
        // public string? UnitName { get; set; } 
    }
}