using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal
{
    // US-GL01 — reverse a POSTED voucher: creates a mirror (Dr/Cr swapped) entry, auto-posts it, links it
    // back via ReversalOfId, and marks the original REVERSED. Returns the reversal's post result.
    public sealed class ReverseJournalCommand : IRequest<ApiResponseDTO<PostJournalResultDto>>, IRequirePermission
    {
        public int Id { get; set; }                  // the posted voucher to reverse
        public DateOnly? ReversalDate { get; set; }  // optional — defaults to the first day of the next open period
        public string? Narration { get; set; }       // optional; always prefixed "Reversal of {VoucherNo}"

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
