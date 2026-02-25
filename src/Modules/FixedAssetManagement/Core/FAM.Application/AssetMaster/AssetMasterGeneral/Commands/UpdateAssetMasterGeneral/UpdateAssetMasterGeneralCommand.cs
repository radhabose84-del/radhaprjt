
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral
{
    public class UpdateAssetMasterGeneralCommand : IRequest<bool>
    {
        public AssetMasterUpdateDto? AssetMaster { get; set; }   
    }
}