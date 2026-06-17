using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeGlMappingSummary
{
    // Tax Code Registry summary: each tax code with its current rate and GL-account mapping count.
    public class GetTaxCodeGlMappingSummaryQuery : IRequest<ApiResponseDTO<List<TaxCodeGlMappingSummaryDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? TaxType { get; set; }
    }
}
