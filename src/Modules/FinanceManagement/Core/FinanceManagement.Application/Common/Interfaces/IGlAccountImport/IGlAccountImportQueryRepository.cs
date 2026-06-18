using FinanceManagement.Application.GlAccountImport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGlAccountImport
{
    public interface IGlAccountImportQueryRepository
    {
        /// <summary>Loads all master/reference data for validation in one shot (company-scoped).</summary>
        Task<GlAccountImportReferenceData> LoadReferenceDataAsync(int companyId, CancellationToken ct);

        /// <summary>Full COA (groups then accounts) as code-based export rows for the company.</summary>
        Task<IReadOnlyList<GlAccountImportRowDto>> GetExportRowsAsync(int companyId, CancellationToken ct);

        /// <summary>Paged import-log history for the company.</summary>
        Task<(List<GlAccountImportLogDto> Logs, int TotalCount)> GetLogsAsync(int companyId, int pageNumber, int pageSize);

        /// <summary>Row-error report for a single import run.</summary>
        Task<IReadOnlyList<GlAccountImportErrorDto>> GetErrorsAsync(int importLogId);

        /// <summary>Guards activate-batch / error fetch against cross-company access.</summary>
        Task<bool> LogBelongsToCompanyAsync(int importLogId, int companyId);
    }
}
