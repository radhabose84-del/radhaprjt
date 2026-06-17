using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMaster
{
    public class GetAllGstrSectionMasterQuery : IRequest<ApiResponseDTO<List<GstrSectionMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? ReportTypeId { get; set; }
    }
}
