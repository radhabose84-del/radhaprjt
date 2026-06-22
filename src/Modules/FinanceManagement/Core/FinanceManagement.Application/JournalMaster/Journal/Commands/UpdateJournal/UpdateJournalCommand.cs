using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal
{
    public class UpdateJournalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int VoucherTypeId { get; set; }
        public DateOnly VoucherDate { get; set; }
        public string? Narration { get; set; }
        public List<JournalLineInputDto> Lines { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
