using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIII
{
    // Validates parsed rows, then creates the sections + their line items in one transaction.
    // No partial commit: if ANY row fails validation, nothing is created and a row-level error report is returned.
    public class ImportScheduleIIICommandHandler : IRequestHandler<ImportScheduleIIICommand, ApiResponseDTO<ImportScheduleIIIResultDto>>
    {
        private const int MaxRows = 5000;

        private readonly IScheduleIIIImportCommandRepository _commandRepository;
        private readonly IScheduleIIIImportQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public ImportScheduleIIICommandHandler(
            IScheduleIIIImportCommandRepository commandRepository,
            IScheduleIIIImportQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ImportScheduleIIIResultDto>> Handle(ImportScheduleIIICommand request, CancellationToken cancellationToken)
        {
            var rows = request.Rows ?? new List<ScheduleIIIImportRowInputDto>();
            var errors = new List<ScheduleIIIImportErrorDto>();

            if (rows.Count == 0)
                errors.Add(Err(0, null, "Import file contains no rows."));
            if (rows.Count > MaxRows)
                errors.Add(Err(0, null, $"Import exceeds the maximum of {MaxRows} rows."));

            // Resolve MiscMaster options (by code OR description) once.
            var stmtMap = ToMap(await _queryRepository.GetStatementTypeOptionsAsync());
            var natureMap = ToMap(await _queryRepository.GetNatureOptionsAsync());

            // Section names that already exist (block duplicate creation).
            var sectionNames = rows.Where(r => !string.IsNullOrWhiteSpace(r.SectionName))
                .Select(r => r.SectionName!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var existing = (await _queryRepository.GetExistingSectionNamesAsync(sectionNames))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // --- Row-level validation ---
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.SectionName))
                    errors.Add(Err(r.RowNo, "SectionName", "Section name is required."));
                if (string.IsNullOrWhiteSpace(r.StatementType) || ResolveId(stmtMap, r.StatementType) == null)
                    errors.Add(Err(r.RowNo, "StatementType", "Statement type does not exist."));
                if (string.IsNullOrWhiteSpace(r.Nature) || ResolveId(natureMap, r.Nature) == null)
                    errors.Add(Err(r.RowNo, "Nature", "Nature does not exist."));
                if (string.IsNullOrWhiteSpace(r.LineCode))
                    errors.Add(Err(r.RowNo, "LineCode", "Line code is required."));
                if (string.IsNullOrWhiteSpace(r.LineName))
                    errors.Add(Err(r.RowNo, "LineName", "Line name is required."));
            }

            // --- Section-level validation (grouped by SectionName) ---
            foreach (var g in rows.Where(r => !string.IsNullOrWhiteSpace(r.SectionName))
                                   .GroupBy(r => r.SectionName!, StringComparer.OrdinalIgnoreCase))
            {
                var first = g.First();

                if (existing.Contains(g.Key))
                    errors.Add(Err(first.RowNo, "SectionName", $"Section '{g.Key}' already exists."));

                var stmtIds = g.Select(r => ResolveId(stmtMap, r.StatementType)).Where(x => x != null).Distinct().ToList();
                if (stmtIds.Count > 1)
                    errors.Add(Err(first.RowNo, "StatementType", $"Section '{g.Key}' has conflicting statement types across its rows."));

                var natureIds = g.Select(r => ResolveId(natureMap, r.Nature)).Where(x => x != null).Distinct().ToList();
                if (natureIds.Count > 1)
                    errors.Add(Err(first.RowNo, "Nature", $"Section '{g.Key}' has conflicting natures across its rows."));

                var dupCodes = g.Where(r => !string.IsNullOrWhiteSpace(r.LineCode))
                    .GroupBy(r => r.LineCode!, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                foreach (var code in dupCodes)
                    errors.Add(Err(first.RowNo, "LineCode", $"Duplicate line code '{code}' in section '{g.Key}'."));
            }

            // --- No partial commit ---
            if (errors.Count > 0)
            {
                await PublishAudit("Import", "SCHEDULEIII_IMPORT_FAILED", request.FileName,
                    $"Schedule III import '{request.FileName}' failed validation with {errors.Count} error(s).", cancellationToken);

                return new ApiResponseDTO<ImportScheduleIIIResultDto>
                {
                    IsSuccess = false,
                    Message = "Import failed validation. Nothing was created.",
                    Data = new ImportScheduleIIIResultDto
                    {
                        TotalRows = rows.Count, ErrorRows = errors.Count, Status = "FAILED", Committed = false, Errors = errors
                    }
                };
            }

            // --- Build graph (one section + its items per SectionName) and commit ---
            // Sequence imported sections after any existing catalog sections, in file (first-appearance) order.
            var baseOrder = await _queryRepository.GetMaxSectionDisplayOrderAsync();
            var graph = rows.GroupBy(r => r.SectionName!, StringComparer.OrdinalIgnoreCase)
                .Select((g, index) =>
                {
                    var first = g.First();
                    var section = new ScheduleIIISection
                    {
                        SectionName = first.SectionName,
                        StatementTypeId = ResolveId(stmtMap, first.StatementType)!.Value,
                        NatureId = ResolveId(natureMap, first.Nature)!.Value,
                        DisplayOrder = baseOrder + index + 1,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };
                    var items = g.Select(r => new ScheduleIIISectionItem
                    {
                        LineCode = r.LineCode,
                        LineName = r.LineName,
                        NoteReference = r.NoteReference,
                        IsSplitLine = r.IsSplitLine,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }).ToList();
                    return (section, items);
                })
                .ToList();

            var (sectionsCreated, itemsCreated, createdIds) = await _commandRepository.CommitAsync(graph, cancellationToken);

            await PublishAudit("Import", "SCHEDULEIII_IMPORT_COMMITTED", request.FileName,
                $"Schedule III import '{request.FileName}' created {sectionsCreated} section(s) and {itemsCreated} line item(s).", cancellationToken);

            return new ApiResponseDTO<ImportScheduleIIIResultDto>
            {
                IsSuccess = true,
                Message = $"Import created {sectionsCreated} section(s) and {itemsCreated} line item(s).",
                Data = new ImportScheduleIIIResultDto
                {
                    TotalRows = rows.Count,
                    SectionsCreated = sectionsCreated,
                    ItemsCreated = itemsCreated,
                    ErrorRows = 0,
                    Status = "COMMITTED",
                    Committed = true,
                    CreatedSectionIds = createdIds
                }
            };
        }

        private static ScheduleIIIImportErrorDto Err(int rowNo, string? column, string message) =>
            new() { RowNo = rowNo, ColumnName = column, Message = message };

        // Build a case-insensitive lookup keyed by BOTH code and description → MiscMaster id.
        private static Dictionary<string, int> ToMap(IReadOnlyList<ScheduleIIIMiscOptionDto> options)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in options)
            {
                if (!string.IsNullOrWhiteSpace(o.Code)) map[o.Code!.Trim()] = o.Id;
                if (!string.IsNullOrWhiteSpace(o.Description)) map[o.Description!.Trim()] = o.Id;
            }
            return map;
        }

        private static int? ResolveId(Dictionary<string, int> map, string? text) =>
            !string.IsNullOrWhiteSpace(text) && map.TryGetValue(text.Trim(), out var id) ? id : null;

        private async Task PublishAudit(string detail, string code, string? name, string details, CancellationToken ct) =>
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: detail, actionCode: code, actionName: name ?? "import", details: details, module: "ScheduleIIIImport"), ct);
    }
}
