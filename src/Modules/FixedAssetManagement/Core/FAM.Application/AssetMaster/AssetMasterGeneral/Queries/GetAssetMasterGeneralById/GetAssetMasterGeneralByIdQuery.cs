using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class GetAssetMasterGeneralByIdQuery : IRequest<AssetMasterDTO>
    {
        public int Id { get; set; }
    }
}