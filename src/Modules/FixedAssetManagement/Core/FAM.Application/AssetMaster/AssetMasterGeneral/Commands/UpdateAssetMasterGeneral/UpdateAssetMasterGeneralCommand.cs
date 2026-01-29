
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using MediatR;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral
{
    public class UpdateAssetMasterGeneralCommand : IRequest<bool>
    {
        public AssetMasterUpdateDto? AssetMaster { get; set; }   
    }
}