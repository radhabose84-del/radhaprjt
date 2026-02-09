using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
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