using Contracts.Common;
using FinanceManagement.Application.AccountTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAllAccountTypeMaster
{
    public class GetAllAccountTypeMasterQuery : IRequest<ApiResponseDTO<List<AccountTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? CompanyId { get; set; }
    }
}
