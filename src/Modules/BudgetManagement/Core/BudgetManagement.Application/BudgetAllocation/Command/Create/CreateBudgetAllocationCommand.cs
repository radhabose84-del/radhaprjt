using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Command.Create
{
    public class CreateBudgetAllocationCommand : IRequest<int>  
    {
        public List<CreateBudgetAllocationDto> createBudgetAllocations { get; set; } = new();
    }
}