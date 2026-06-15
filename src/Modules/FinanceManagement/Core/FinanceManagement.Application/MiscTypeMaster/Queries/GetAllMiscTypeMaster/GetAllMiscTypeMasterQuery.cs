using Contracts.Common;
using FinanceManagement.Application.MiscTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQuery : IRequest<ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
