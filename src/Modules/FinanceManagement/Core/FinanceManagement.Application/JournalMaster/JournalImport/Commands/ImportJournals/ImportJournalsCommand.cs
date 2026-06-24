using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals
{
    public class ImportJournalsCommand : IRequest<ApiResponseDTO<ImportJournalsResultDto>>, IRequirePermission
    {
        public string? FileName { get; set; }
        public List<JournalImportRowInputDto> Rows { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
