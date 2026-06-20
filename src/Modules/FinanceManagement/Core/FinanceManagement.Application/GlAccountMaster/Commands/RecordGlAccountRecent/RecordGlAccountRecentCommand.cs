using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.RecordGlAccountRecent
{
    // Record "the user just picked this account in an entry" (US-GL02-07 record-on-select).
    // Drives recently-used ranking. User + company from the token.
    public class RecordGlAccountRecentCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int GlAccountMasterId { get; set; }
    }
}
