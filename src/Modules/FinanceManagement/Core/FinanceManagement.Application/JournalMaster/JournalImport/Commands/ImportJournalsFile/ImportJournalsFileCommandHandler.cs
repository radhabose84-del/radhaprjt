using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournalsFile
{
    public class ImportJournalsFileCommandHandler
        : IRequestHandler<ImportJournalsFileCommand, ApiResponseDTO<ImportJournalsResultDto>>
    {
        private const long MaxFileBytes = 10 * 1024 * 1024; // 10 MB

        private readonly IJournalImportFileService _fileService;
        private readonly IMediator _mediator;

        public ImportJournalsFileCommandHandler(IJournalImportFileService fileService, IMediator mediator)
        {
            _fileService = fileService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ImportJournalsResultDto>> Handle(ImportJournalsFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return Fail("No file was uploaded.");
            if (request.File.Length > MaxFileBytes)
                return Fail("File size exceeds the 10 MB limit.");
            if (!_fileService.IsSupported(request.File.FileName))
                return Fail("Unsupported file type. Upload a .xlsx or .csv file.");

            // 1. Parse the workbook/CSV into typed rows.
            IReadOnlyList<JournalImportRowInputDto> rows;
            IReadOnlyList<JournalImportErrorDto> parseErrors;
            using (var stream = new MemoryStream())
            {
                await request.File.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                (rows, parseErrors) = _fileService.Parse(stream, request.File.FileName);
            }

            // 2. Malformed file (bad structure / unparseable cells) → reject before touching the DB.
            if (parseErrors.Count > 0)
            {
                return new ApiResponseDTO<ImportJournalsResultDto>
                {
                    IsSuccess = false,
                    Message = "The file could not be parsed. Fix the highlighted rows and re-upload.",
                    Data = new ImportJournalsResultDto
                    {
                        TotalRows = rows.Count,
                        ErrorRows = parseErrors.Count,
                        Status = "FAILED",
                        Committed = false,
                        Errors = parseErrors.ToList()
                    }
                };
            }

            // 3. Delegate to the standard import (FK + balance + period validation → commit or failed batch).
            return await _mediator.Send(new ImportJournalsCommand
            {
                FileName = request.File.FileName,
                Rows = rows.ToList()
            }, cancellationToken);
        }

        private static ApiResponseDTO<ImportJournalsResultDto> Fail(string message) =>
            new() { IsSuccess = false, Message = message, Data = null };
    }
}
