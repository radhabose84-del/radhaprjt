using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountSearch
{
    // US-GL02-07 reusable account type-ahead. Company + user come from the token.
    // Empty Term → the user's favourites + recently-used (shortcuts); otherwise a ranked search.
    public class GetGlAccountSearchQuery : IRequest<IReadOnlyList<AccountSearchResultDto>>
    {
        public string? Term { get; set; }
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
        public bool ActiveOnly { get; set; }
        public int Take { get; set; } = 20;
    }
}
