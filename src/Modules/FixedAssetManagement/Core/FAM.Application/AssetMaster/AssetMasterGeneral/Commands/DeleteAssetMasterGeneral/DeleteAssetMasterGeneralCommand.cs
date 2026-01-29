
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral
{
    public class DeleteAssetMasterGeneralCommand :  IRequest<AssetMasterGeneralDTO>
    {
        public int Id { get; set; }     
    }
}