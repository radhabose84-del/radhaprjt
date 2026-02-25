using Contracts.Common;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise
{
    public class GetSpindleDetailsMonthwiseQuery : IRequest<ApiResponseDTO<List<GetSpindleDetailsMonthwiseDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}