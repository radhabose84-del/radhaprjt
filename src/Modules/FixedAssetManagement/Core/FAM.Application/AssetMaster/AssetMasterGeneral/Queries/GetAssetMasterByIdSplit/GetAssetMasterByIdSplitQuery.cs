using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterByIdSplit
{
    public class GetAssetMasterByIdSplitQuery : IRequest<AssetMasterSplitDto>
    {
        public int Id { get; set; }
    }
}