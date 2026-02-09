using MediatR;

namespace  BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending
{
    public sealed class GetBudgetRequestPendingQuery
        : IRequest<(List<GetBudgetRequestPendingDto> Items, int TotalCount)>     
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }                     
    }
}

    