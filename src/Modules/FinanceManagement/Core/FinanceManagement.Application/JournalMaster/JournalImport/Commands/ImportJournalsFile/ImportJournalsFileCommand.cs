using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournalsFile
{
    // US-GL01-17 — browse + upload an Excel/CSV file; the handler parses it into rows and runs the
    // standard journal import (validate → commit drafts, or record a failed batch).
    public sealed class ImportJournalsFileCommand : IRequest<ApiResponseDTO<ImportJournalsResultDto>>, IRequirePermission
    {
        public IFormFile? File { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
