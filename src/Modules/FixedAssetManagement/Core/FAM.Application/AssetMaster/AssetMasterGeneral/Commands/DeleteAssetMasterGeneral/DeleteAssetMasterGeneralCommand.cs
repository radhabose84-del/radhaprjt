
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral
{
    public class DeleteAssetMasterGeneralCommand :  IRequest<AssetMasterGeneralDTO>
    {
        public int Id { get; set; }     
    }
}