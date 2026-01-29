using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.SaveAssetDocument
{
    public class SaveAssetDocumentCommand :  IRequest<bool>
    {
        public int Id { get; set; }
        public string? AssetCode { get; set; }  
        public string? assetPath { get; set; }          

    }
}
