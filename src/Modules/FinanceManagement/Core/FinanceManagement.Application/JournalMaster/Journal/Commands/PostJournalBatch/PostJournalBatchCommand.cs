using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch
{
    // US-GL01-09 — best-effort bulk posting: each id is posted independently so one bad voucher
    // never blocks the rest. Returns a per-voucher result list.
    public sealed class PostJournalBatchCommand : IRequest<ApiResponseDTO<List<PostJournalBatchItemDto>>>, IRequirePermission
    {
        public List<int> Ids { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
