using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.Common.HttpResponse;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById
{
    public class GetBudgetGroupByIdQuery : IRequest<ApiResponseDTO<BudgetGroupDto>>
    {
        public int Id { get; set; }
    }
}