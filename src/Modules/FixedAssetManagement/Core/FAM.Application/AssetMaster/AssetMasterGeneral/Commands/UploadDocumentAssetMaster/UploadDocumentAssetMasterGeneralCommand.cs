using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster
{
    public class UploadDocumentAssetMasterGeneralCommand : IRequest<AssetMasterDocumentDto>
    {
        public IFormFile? File { get; set; }    
    }
}
