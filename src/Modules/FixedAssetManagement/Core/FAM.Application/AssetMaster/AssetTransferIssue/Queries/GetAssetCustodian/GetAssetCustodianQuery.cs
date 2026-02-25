using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian
{
    public class GetAssetCustodianQuery : IRequest<List<GetAssetCustodianDto>>
    {
        public string? OldUnitId { get; set; }
        
        public int DepartmentId { get; set; }
    }
}