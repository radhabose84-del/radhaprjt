using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals
{
    // US-GL01-17 — validates parsed rows, then commits valid rows as Draft journals (source = IMPORT).
    // No partial commit: if ANY row fails validation, nothing is committed and a row-level error report is returned.
    public class ImportJournalsCommandHandler : IRequestHandler<ImportJournalsCommand, ApiResponseDTO<ImportJournalsResultDto>>
    {
        private const int MaxRows = 5000;

        private readonly IJournalImportCommandRepository _commandRepository;
        private readonly IJournalImportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ImportJournalsCommandHandler(
            IJournalImportCommandRepository commandRepository,
            IJournalImportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ImportJournalsResultDto>> Handle(ImportJournalsCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var unitId = _ipAddressService.GetUnitId();

            var importSourceId = await _queryRepository.GetSourceIdAsync("IMPORT");

            var rows = request.Rows ?? new List<JournalImportRowInputDto>();
            var errors = new List<JournalImportError>();

            if (rows.Count == 0)
                errors.Add(new JournalImportError { RowNo = 0, ColumnName = null, Message = "Import file contains no rows." });
            if (rows.Count > MaxRows)
                errors.Add(new JournalImportError { RowNo = 0, ColumnName = null, Message = $"Import exceeds the maximum of {MaxRows} lines." });

            // Bulk-load valid FK ids once.
            var glOk = (await _queryRepository.GetExistingGlAccountIdsAsync(rows.Select(r => r.GlAccountId), companyId)).ToHashSet();
            var vtOk = (await _queryRepository.GetExistingVoucherTypeIdsAsync(rows.Select(r => r.VoucherTypeId), companyId)).ToHashSet();
            var ccOk = (await _queryRepository.GetExistingCostCentreIdsAsync(rows.Where(r => r.CostCentreId.HasValue).Select(r => r.CostCentreId!.Value))).ToHashSet();
            var pcOk = (await _queryRepository.GetExistingProfitCentreIdsAsync(rows.Where(r => r.ProfitCentreId.HasValue).Select(r => r.ProfitCentreId!.Value))).ToHashSet();
            var curOk = (await _queryRepository.GetExistingCurrencyIdsAsync(rows.Select(r => r.CurrencyId))).ToHashSet();

            // Resolve open period per distinct (date) once.
            var periodByDate = new Dictionary<DateOnly, (int PeriodId, int FinancialYearId)?>();
            foreach (var d in rows.Select(r => r.VoucherDate).Distinct())
                periodByDate[d] = await _queryRepository.GetOpenPeriodByDateAsync(companyId, d);

            // --- Row-level validation ---
            foreach (var r in rows)
            {
                if (r.VoucherTypeId <= 0 || !vtOk.Contains(r.VoucherTypeId))
                    errors.Add(Err(r.RowNo, "voucher_type", "Voucher type does not exist."));
                if (r.GlAccountId <= 0 || !glOk.Contains(r.GlAccountId))
                    errors.Add(Err(r.RowNo, "gl_account", "GL account does not exist or is inactive."));
                if (r.CurrencyId <= 0 || !curOk.Contains(r.CurrencyId))
                    errors.Add(Err(r.RowNo, "currency", "Currency does not exist."));
                if (r.CostCentreId.HasValue && r.CostCentreId.Value > 0 && !ccOk.Contains(r.CostCentreId.Value))
                    errors.Add(Err(r.RowNo, "cost_centre", "Cost centre does not exist."));
                if (r.ProfitCentreId.HasValue && r.ProfitCentreId.Value > 0 && !pcOk.Contains(r.ProfitCentreId.Value))
                    errors.Add(Err(r.RowNo, "profit_centre", "Profit centre does not exist."));
                if (!((r.DrAmount > 0) ^ (r.CrAmount > 0)))
                    errors.Add(Err(r.RowNo, "dr/cr", "Each row must be either a debit or a credit."));
                if (periodByDate.TryGetValue(r.VoucherDate, out var p) && p == null)
                    errors.Add(Err(r.RowNo, "voucher_date", "Voucher date is not within an open accounting period."));
            }

            // --- Group-level balance validation (Dr = Cr per voucher group) ---
            foreach (var g in rows.GroupBy(r => r.GroupNo))
            {
                var dr = g.Sum(x => x.DrAmount);
                var cr = g.Sum(x => x.CrAmount);
                if (dr != cr || dr <= 0)
                    errors.Add(Err(g.Min(x => x.RowNo), "dr/cr", $"Voucher group {g.Key} is out of balance (Dr {dr} ≠ Cr {cr})."));
            }

            // --- No partial commit: any error → record the batch + errors, commit no journals ---
            if (errors.Count > 0)
            {
                var failedBatch = BuildBatch(request, rows.Count, 0, errors.Count, await _queryRepository.GetBatchStatusIdAsync("FAILED"), importSourceId);
                var batchId = await _commandRepository.SaveFailedBatchAsync(failedBatch, errors, cancellationToken);

                await PublishAudit("Import", "JOURNAL_IMPORT_FAILED", batchId, $"Import '{request.FileName}' failed validation with {errors.Count} error(s).", cancellationToken);

                return new ApiResponseDTO<ImportJournalsResultDto>
                {
                    IsSuccess = false,
                    Message = "Import failed validation. No journals were committed.",
                    Data = ToResult(batchId, rows.Count, 0, errors.Count, "FAILED", false, errors, new List<int>())
                };
            }

            // --- All valid → build draft journals (one per group) and commit in one transaction ---
            var draftStatusId = await _queryRepository.GetStatusIdAsync("DRAFT");

            var drafts = rows.GroupBy(r => r.GroupNo)
                .Select(g => BuildDraft(g.ToList(), companyId, unitId, draftStatusId, importSourceId, periodByDate))
                .ToList();

            var committedBatch = BuildBatch(request, rows.Count, rows.Count, 0, await _queryRepository.GetBatchStatusIdAsync("COMMITTED"), importSourceId);
            var (committedBatchId, journalIds) = await _commandRepository.CommitAsync(committedBatch, drafts, cancellationToken);

            await PublishAudit("Import", "JOURNAL_IMPORT_COMMITTED", committedBatchId, $"Import '{request.FileName}' committed {journalIds.Count} draft journal(s).", cancellationToken);

            return new ApiResponseDTO<ImportJournalsResultDto>
            {
                IsSuccess = true,
                Message = $"Import committed {journalIds.Count} draft journal(s).",
                Data = ToResult(committedBatchId, rows.Count, rows.Count, 0, "COMMITTED", true, new List<JournalImportError>(), journalIds)
            };
        }

        private static JournalImportError Err(int rowNo, string column, string message) =>
            new() { RowNo = rowNo, ColumnName = column, Message = message };

        private JournalImportBatch BuildBatch(ImportJournalsCommand request, int total, int valid, int error, int statusId, int sourceId) =>
            new()
            {
                FileName = request.FileName ?? "import.xlsx",
                TotalRows = total,
                ValidRows = valid,
                ErrorRows = error,
                StatusId = statusId,
                SourceId = sourceId,
                ImportedBy = _ipAddressService.GetUserId(),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private static JournalHeader BuildDraft(
            List<JournalImportRowInputDto> group, int companyId, int? unitId, int draftStatusId, int importSourceId,
            Dictionary<DateOnly, (int PeriodId, int FinancialYearId)?> periodByDate)
        {
            var first = group[0];
            var period = periodByDate[first.VoucherDate]!.Value;
            var lineNo = 0;

            return new JournalHeader
            {
                CompanyId = companyId,
                UnitId = unitId,
                VoucherTypeId = first.VoucherTypeId,
                VoucherDate = first.VoucherDate,
                FinancialYearId = period.FinancialYearId,
                AccountingPeriodId = period.PeriodId,
                Narration = first.Narration,
                StatusId = draftStatusId,
                SourceId = importSourceId,
                IsReversal = false,
                AutoApproved = false,
                TotalDr = group.Sum(x => x.DrAmount),
                TotalCr = group.Sum(x => x.CrAmount),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = group.Select(r => new JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = r.GlAccountId,
                    DrAmount = r.DrAmount,
                    CrAmount = r.CrAmount,
                    CurrencyId = r.CurrencyId,
                    ExchangeRate = 1m,
                    BaseDrAmount = r.DrAmount,
                    BaseCrAmount = r.CrAmount,
                    CostCentreId = r.CostCentreId,
                    ProfitCentreId = r.ProfitCentreId,
                    LineNarration = r.LineNarration,
                    ReferenceDocNo = r.ReferenceDocNo
                }).ToList()
            };
        }

        private static ImportJournalsResultDto ToResult(int batchId, int total, int valid, int error, string status, bool committed, List<JournalImportError> errors, List<int> journalIds) =>
            new()
            {
                BatchId = batchId,
                TotalRows = total,
                ValidRows = valid,
                ErrorRows = error,
                Status = status,
                Committed = committed,
                Errors = errors.Select(e => new JournalImportErrorDto { RowNo = e.RowNo, ColumnName = e.ColumnName, Message = e.Message }).ToList(),
                CreatedJournalIds = journalIds
            };

        private async Task PublishAudit(string detail, string code, int batchId, string details, CancellationToken ct)
        {
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: detail, actionCode: code, actionName: batchId.ToString(), details: details, module: "JournalImport"), ct);
        }
    }
}
