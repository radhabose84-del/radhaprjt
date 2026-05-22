using MediatR;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using Contracts.Common;

namespace UserManagement.Application.FinancialYear.Command.CreateFinancialYear
{
    public class CreateFinancialYearCommand : IRequest<FinancialYearDto>, IRequirePermission
    {

        public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? FinYearName { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
