using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport
{
    public class GetBudgetBalanceReportQuery  : IRequest<List<BudgetBalanceReportDto>>
    {
         public int FinancialYearId { get; set; }
    }
}