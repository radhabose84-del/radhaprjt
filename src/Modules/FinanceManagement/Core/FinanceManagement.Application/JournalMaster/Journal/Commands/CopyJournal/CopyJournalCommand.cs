using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal
{
    // US-GL01 — duplicate an existing journal into a NEW editable draft (blank voucher no, today's date,
    // identical lines, status Draft). The copy is independent — no posting link to the source.
    public sealed class CopyJournalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }   // the source voucher to copy

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
