using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringGeneratedInstances
{
    // "Generated Instances" grid — journals created from recurring templates (company-scoped via the token).
    public class GetRecurringGeneratedInstancesQuery : IRequest<ApiResponseDTO<List<RecurringGeneratedInstanceDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
