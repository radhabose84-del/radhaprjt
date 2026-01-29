using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteDocumentAssetMasterGeneral
{
    public class DeleteDocumentAssetMasterGeneralCommand : IRequest<bool>
    {
        public string? assetPath { get; set; }       
    }
}