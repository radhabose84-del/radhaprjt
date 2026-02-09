using System;
using System.Collections.Generic;
using System.Linq;
using BudgetManagement.Application.BudgetGroups;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupAutoComplete
{
    public class GetBudgetGroupAutoCompleteQuery : IRequest<List<BudgetGroupAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        
    }
}