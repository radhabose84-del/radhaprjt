using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeEffective
{
    public class GetTaxCodeEffectiveQuery : IRequest<ApiResponseDTO<TaxCodeMasterDto?>>
    {
        public string Code { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public DateOnly AsOf { get; set; }
    }
}
