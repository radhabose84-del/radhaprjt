using MediatR;
using Contracts.Common;

namespace FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail
{
    public class CreateDepreciationDetailCommand  : IRequest<string>, IRequirePermission 
    {
        public int companyId { get; set; } 
        public int unitId { get; set; } 
        public int finYearId { get; set; }        
        public int depreciationType { get; set; } 
        public int depreciationPeriod { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
