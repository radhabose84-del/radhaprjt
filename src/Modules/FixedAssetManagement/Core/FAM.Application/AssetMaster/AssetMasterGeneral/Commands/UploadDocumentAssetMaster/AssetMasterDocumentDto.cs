using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster
{
    public class AssetMasterDocumentDto : IMapFrom<AssetMasterGenerals>
    {
        public string? AssetDocument { get; set; }
        public string? AssetDocumentBase64 { get; set; }
    }
}