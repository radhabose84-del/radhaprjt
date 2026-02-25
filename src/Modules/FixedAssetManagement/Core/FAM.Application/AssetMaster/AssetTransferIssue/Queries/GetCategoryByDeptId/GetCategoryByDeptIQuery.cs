using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId
{
    public class GetCategoryByDeptIQuery  :  IRequest<List<GetCategoryByDeptIdDto>>
    {    

        public int DepartmentId { get; set; }

        
    }
}