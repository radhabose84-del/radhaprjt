using Contracts.Common;
using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetCoaConsistencyReport
{
    // US-GL02-10 (AC4) — accounts that exist in only one company of the current entity group.
    public class GetCoaConsistencyReportQuery : IRequest<ApiResponseDTO<List<CoaConsistencyReportItemDto>>>
    {
    }
}
