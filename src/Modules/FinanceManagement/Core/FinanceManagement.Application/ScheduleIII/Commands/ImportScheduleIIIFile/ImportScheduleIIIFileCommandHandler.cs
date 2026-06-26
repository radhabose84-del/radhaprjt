using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIIIFile
{
    public class ImportScheduleIIIFileCommandHandler
        : IRequestHandler<ImportScheduleIIIFileCommand, ApiResponseDTO<ImportScheduleIIIResultDto>>
    {
        private const long MaxFileBytes = 10 * 1024 * 1024; // 10 MB

        private readonly IScheduleIIIImportFileService _fileService;
        private readonly IMediator _mediator;

        public ImportScheduleIIIFileCommandHandler(IScheduleIIIImportFileService fileService, IMediator mediator)
        {
            _fileService = fileService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ImportScheduleIIIResultDto>> Handle(ImportScheduleIIIFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
                return Fail("No file was uploaded.");
            if (request.File.Length > MaxFileBytes)
                return Fail("File size exceeds the 10 MB limit.");
            if (!_fileService.IsSupported(request.File.FileName))
                return Fail("Unsupported file type. Upload a .xlsx or .csv file.");

            IReadOnlyList<ScheduleIIIImportRowInputDto> rows;
            IReadOnlyList<ScheduleIIIImportErrorDto> parseErrors;
            using (var stream = new MemoryStream())
            {
                await request.File.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                (rows, parseErrors) = _fileService.Parse(stream, request.File.FileName);
            }

            if (parseErrors.Count > 0)
            {
                return new ApiResponseDTO<ImportScheduleIIIResultDto>
                {
                    IsSuccess = false,
                    Message = "The file could not be parsed. Fix the highlighted rows and re-upload.",
                    Data = new ImportScheduleIIIResultDto
                    {
                        TotalRows = rows.Count,
                        ErrorRows = parseErrors.Count,
                        Status = "FAILED",
                        Committed = false,
                        Errors = parseErrors.ToList()
                    }
                };
            }

            return await _mediator.Send(new ImportScheduleIIICommand
            {
                FileName = request.File.FileName,
                Rows = rows.ToList()
            }, cancellationToken);
        }

        private static ApiResponseDTO<ImportScheduleIIIResultDto> Fail(string message) =>
            new() { IsSuccess = false, Message = message, Data = null };
    }
}
