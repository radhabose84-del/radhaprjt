using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Commands.Delete;
public class DeleteBudgetRequestCommand : IRequest
{
    public int Id { get; set; }
}