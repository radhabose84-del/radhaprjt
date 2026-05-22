using MediatR;
using Contracts.Common;

namespace UserManagement.Application.FinancialYear.Command.UpdateFinancialYear
{
    public class UpdateFinancialYearCommand : IRequest<int>, IRequirePermission
    {

         public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? FinYearName { get; set; } 
        
        public byte IsActive { get; set; }



       
       
        
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
