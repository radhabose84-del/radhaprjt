using MediatR;


namespace BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup
{
    public class DeleteBudgetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
