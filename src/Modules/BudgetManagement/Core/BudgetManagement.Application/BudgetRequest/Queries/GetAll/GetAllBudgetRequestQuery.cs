using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Queries.GetAll;

public class GetAllBudgetRequestQuery : IRequest<(IReadOnlyList<BudgetRequestListItemDto> Items, int Total)>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize  { get; set; } = 20;
    public int? StatusId { get; set; }
    public string? SearchTerm { get; set; }
}