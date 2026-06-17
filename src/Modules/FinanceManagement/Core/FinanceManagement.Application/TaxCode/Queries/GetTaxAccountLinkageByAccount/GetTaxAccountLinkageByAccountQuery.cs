using Contracts.Common;
using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageByAccount
{
    public class GetTaxAccountLinkageByAccountQuery : IRequest<ApiResponseDTO<TaxAccountLinkageDto?>>
    {
        public int GlAccountId { get; set; }
    }
}
