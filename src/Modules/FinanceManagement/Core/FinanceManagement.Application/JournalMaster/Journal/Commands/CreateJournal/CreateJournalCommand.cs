using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal
{
    public class CreateJournalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        // CompanyId / UnitId taken from the session token; period + fiscal year resolved from VoucherDate.
        public int VoucherTypeId { get; set; }
        public DateOnly VoucherDate { get; set; }
        public string? Narration { get; set; }
        public List<JournalLineInputDto> Lines { get; set; } = new();

        // Set true to bypass the "possible duplicate voucher" check (same date/amount/lines already exist).
        public bool OverrideDuplicate { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
