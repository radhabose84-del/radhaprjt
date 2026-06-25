using Contracts.Common;
using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetSelectableCompanies
{
    // US-GL02-10 (AC5) — companies the current user may pick in the mandatory company selector.
    public class GetSelectableCompaniesQuery : IRequest<ApiResponseDTO<List<CompanyOptionDto>>>
    {
    }
}
