using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountFavourites
{
    // The logged-in user's favourite accounts (US-GL02-07). User + company from the token.
    public sealed record GetGlAccountFavouritesQuery
        : IRequest<IReadOnlyList<AccountSearchResultDto>>;
}
