using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster
{
    public class GetAllTaxCodeMasterQuery : IRequest<ApiResponseDTO<List<TaxCodeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? TaxType { get; set; }
    }
}
