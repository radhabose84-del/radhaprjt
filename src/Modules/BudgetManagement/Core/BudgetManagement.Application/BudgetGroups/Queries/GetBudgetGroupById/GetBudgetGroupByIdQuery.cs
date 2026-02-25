using Contracts.Common;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById
{
    public class GetBudgetGroupByIdQuery : IRequest<ApiResponseDTO<BudgetGroupDto>>
    {
        public int Id { get; set; }
    }
}