using Contracts.Common;
using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetAllGlAccountMaster
{
    public class GetAllGlAccountMasterQuery : IRequest<ApiResponseDTO<List<GlAccountMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
    }
}
