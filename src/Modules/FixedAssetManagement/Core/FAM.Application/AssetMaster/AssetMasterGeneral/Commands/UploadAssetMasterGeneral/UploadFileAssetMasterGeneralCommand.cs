using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadAssetMasterGeneral
{
    public class UploadFileAssetMasterGeneralCommand : IRequest<AssetMasterImageDto>
    {
        public IFormFile? File { get; set; }
        // public string? CompanyName { get; set; }  
        // public string? UnitName { get; set; }  
      /*   public string? AssetCode { get; set; }   */
    }
}
