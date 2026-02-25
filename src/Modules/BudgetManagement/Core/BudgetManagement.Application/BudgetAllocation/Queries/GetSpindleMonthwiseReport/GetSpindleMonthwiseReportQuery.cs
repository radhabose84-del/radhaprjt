using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport
{
    public class GetSpindleMonthwiseReportQuery : IRequest<List<GetSpindleMonthwiseReportDto>>
    {
        public int FinancialYearId { get; set; }
        public int? DepartmentId { get; set; }
        public int? CostCenterId { get; set; }
        public int? AllocationTypeId { get; set; }
        public int? BudgetGroupId { get; set; }
        public DateOnly? BudgetDate { get; set; }
    }
}