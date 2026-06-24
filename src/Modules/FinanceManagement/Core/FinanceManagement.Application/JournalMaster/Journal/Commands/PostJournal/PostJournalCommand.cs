using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal
{
    public sealed record PostJournalCommand(int Id) : IRequest<ApiResponseDTO<PostJournalResultDto>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
