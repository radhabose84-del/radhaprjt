using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite
{
    // Star an account for the logged-in user (US-GL02-07). User + company from the token.
    public class AddGlAccountFavouriteCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int GlAccountMasterId { get; set; }
    }
}
