using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal
{
    // PostedDate = the accounting posting date. Defaults to the current date when omitted; must be on or after the
    // voucher date and never a future date.
    public sealed record PostJournalCommand(int Id, DateOnly? PostedDate = null) : IRequest<ApiResponseDTO<PostJournalResultDto>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
