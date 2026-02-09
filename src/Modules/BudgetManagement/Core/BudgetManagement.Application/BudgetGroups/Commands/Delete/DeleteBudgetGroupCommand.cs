using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;


namespace BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup
{
    public class DeleteBudgetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
