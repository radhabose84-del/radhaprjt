using MediatR;

namespace  BudgetManagement.Application.BudgetRequest.Queries.GetById;

public class GetBudgetRequestByIdQuery : IRequest<BudgetRequestDto>
{
    public int Id { get; set; }
}
