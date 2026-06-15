using Contracts.Common;
using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree
{
    public class GetAccountGroupTreeQuery : IRequest<ApiResponseDTO<List<AccountGroupTreeDto>>>
    {
        // Optional company scope; null returns all companies' groups.
        public int? CompanyId { get; set; }
    }
}
