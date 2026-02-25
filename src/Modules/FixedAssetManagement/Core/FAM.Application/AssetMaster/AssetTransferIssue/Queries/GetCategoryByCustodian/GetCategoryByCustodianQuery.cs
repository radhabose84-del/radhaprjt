using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian
{
    public class GetCategoryByCustodianQuery : IRequest<List<GetCategoryByCustodianDto>>
    {
          public int DepartmentId { get; set; }
        public string? CustodianId { get; set; }      
        
    }
}