using System.Diagnostics;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Application.GlAccountImport.Services;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Commands.ImportGlAccounts
{
    /// <summary>
    /// Dedicated bulk-import handler (deliberately NOT the per-row CreateGlAccountMaster path, which
    /// defaults Active and commits per row). It parses → validates everything → then commits once
    /// (no partial commit, AC1), imports accounts Inactive (AC3), and always retains an import log
    /// + row-error report (AC1/AC2/AC4).
    /// </summary>
    public class ImportGlAccountsCommandHandler
        : IRequestHandler<ImportGlAccountsCommand, ApiResponseDTO<GlAccountImportResultDto>>
    {
        private const long MaxFileBytes = 10 * 1024 * 1024; // 10 MB — comfortably covers 500+ rows

        private readonly IGlAccountImportFileService _fileService;
        private readonly IGlAccountImportValidator _validator;
        private readonly IGlAccountImportQueryRepository _queryRepository;
        private readonly IGlAccountImportCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ImportGlAccountsCommandHandler(
            IGlAccountImportFileService fileService,
            IGlAccountImportValidator validator,
            IGlAccountImportQueryRepository queryRepository,
            IGlAccountImportCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _fileService = fileService;
            _validator = validator;
            _queryRepository = queryRepository;
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<GlAccountImportResultDto>> Handle(
            ImportGlAccountsCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            if (request.File == null || request.File.Length == 0)
                return Fail("No file was uploaded.");

            if (request.File.Length > MaxFileBytes)
                return Fail("File size exceeds the 10 MB limit.");

            if (!_fileService.IsSupported(request.File.FileName))
                return Fail("Unsupported file type. Upload a .xlsx or .csv file.");

            var mode = GlAccountImportModes.Normalize(request.Mode);
            var format = _fileService.ResolveFormat(request.File.FileName);
            var stopwatch = Stopwatch.StartNew();

            // 1. Parse
            IReadOnlyList<GlAccountImportRowDto> rows;
            IReadOnlyList<GlAccountImportErrorDto> parseErrors;
            using (var stream = new MemoryStream())
            {
                await request.File.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                (rows, parseErrors) = _fileService.Parse(stream, format);
            }

            // 2. Validate against reference data loaded in one shot
            var refData = await _queryRepository.LoadReferenceDataAsync(companyId, cancellationToken);
            var validation = _validator.Validate(rows, parseErrors, refData);

            // 3. Decide what to commit
            bool abort = mode == GlAccountImportModes.AllOrNothing && validation.HasErrors;

            var commit = new GlAccountImportCommitRequest
            {
                CompanyId = companyId,
                FileName = request.File.FileName,
                FileFormat = format,
                ImportMode = mode,
                TotalRows = validation.TotalRows,
                GroupRows = validation.GroupRows,
                AccountRows = validation.AccountRows,
                Errors = validation.Errors,
                Groups = abort ? new List<PlannedAccountGroup>() : validation.Groups,
                Accounts = abort ? new List<PlannedGlAccount>() : validation.Accounts,
                Status = abort
                    ? GlAccountImportStatuses.ValidatedWithErrors
                    : validation.HasErrors
                        ? GlAccountImportStatuses.CompletedWithSkips
                        : GlAccountImportStatuses.Completed
            };

            stopwatch.Stop();
            commit.DurationMs = (int)stopwatch.ElapsedMilliseconds;

            // 4. Persist (single transaction: groups + accounts + log + errors)
            var importLogId = await _commandRepository.CommitAsync(commit, cancellationToken);

            var result = new GlAccountImportResultDto
            {
                ImportLogId = importLogId,
                FileName = commit.FileName,
                FileFormat = format,
                ImportMode = mode,
                Status = commit.Status,
                TotalRows = validation.TotalRows,
                GroupRows = validation.GroupRows,
                AccountRows = validation.AccountRows,
                InvalidRows = validation.InvalidRowCount,
                ValidRows = validation.TotalRows - validation.InvalidRowCount,
                ImportedGroups = commit.Groups.Count,
                ImportedAccounts = commit.Accounts.Count,
                SkippedRows = validation.InvalidRowCount,
                DurationMs = commit.DurationMs,
                Errors = validation.Errors
            };

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Import",
                actionCode: "GL_ACCOUNT_IMPORT",
                actionName: importLogId.ToString(),
                details: $"COA import '{commit.FileName}' [{mode}] → status {commit.Status}; " +
                         $"groups {result.ImportedGroups}, accounts {result.ImportedAccounts}, " +
                         $"skipped {result.SkippedRows} of {result.TotalRows} in {result.DurationMs}ms (Company {companyId}).",
                module: "GlAccountImport");
            await _mediator.Publish(auditEvent, cancellationToken);

            var message = abort
                ? $"Import aborted: {result.InvalidRows} row(s) failed validation. No rows were committed. See the error report."
                : validation.HasErrors
                    ? $"Imported {result.ImportedGroups} group(s) and {result.ImportedAccounts} account(s); {result.SkippedRows} row(s) skipped."
                    : $"Imported {result.ImportedGroups} group(s) and {result.ImportedAccounts} account(s) successfully.";

            return new ApiResponseDTO<GlAccountImportResultDto>
            {
                IsSuccess = !abort,
                Message = message,
                Data = result
            };
        }

        private static ApiResponseDTO<GlAccountImportResultDto> Fail(string message) =>
            new() { IsSuccess = false, Message = message, Data = null };
    }

    public static class GlAccountImportStatuses
    {
        public const string ValidatedWithErrors = "ValidatedWithErrors";
        public const string Completed = "Completed";
        public const string CompletedWithSkips = "CompletedWithSkips";
        public const string Failed = "Failed";
    }
}
