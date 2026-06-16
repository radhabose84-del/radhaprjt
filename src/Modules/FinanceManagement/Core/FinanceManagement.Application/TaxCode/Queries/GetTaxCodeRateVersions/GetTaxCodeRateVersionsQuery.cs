using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeRateVersions
{
    public class GetTaxCodeRateVersionsQuery : IRequest<ApiResponseDTO<List<TaxCodeRateVersionDto>>>
    {
        public int TaxCodeId { get; set; }
    }
}
