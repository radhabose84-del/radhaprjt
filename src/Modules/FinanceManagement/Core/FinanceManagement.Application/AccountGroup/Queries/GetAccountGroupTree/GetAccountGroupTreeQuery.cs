using Contracts.Common;
using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree
{
    // Company scope is resolved from the session token in the handler (IIPAddressService.GetCompanyId()).
    public class GetAccountGroupTreeQuery : IRequest<ApiResponseDTO<List<AccountGroupTreeDto>>>
    {
    }
}
