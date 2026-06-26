using Contracts.Common;
using FinanceManagement.Application.CoaRead.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.ValidateForPosting
{
    // US-GL02-16 (AC2) — validate an account for posting: active + currency match + cost-centre rule.
    public class ValidateForPostingQuery : IRequest<ApiResponseDTO<PostingValidationResultDto>>
    {
        public string AccountCode { get; set; } = string.Empty;
        public int? CurrencyId { get; set; }      // the currency the caller intends to post in
        public int? CostCentreId { get; set; }    // the cost centre the caller will supply (if any)
    }
}
