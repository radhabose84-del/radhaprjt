using FinanceManagement.Application.PeriodStatusOverride.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride
{
    public interface IPeriodStatusOverrideQueryRepository
    {
        Task<FinancialPeriodStatusDto?> GetPeriodStatusAsync(int periodId, int companyId, CancellationToken ct);
        Task<IReadOnlyList<PeriodStatusOverrideDto>> GetHistoryForPeriodAsync(int periodId, int companyId, CancellationToken ct);
        Task<IReadOnlyList<PeriodStatusOverrideDto>> GetPendingForCompanyAsync(int companyId, CancellationToken ct);

        Task<PeriodStatusOverrideDto?> GetByIdAsync(int id);

        /// <summary>True if the period has a PENDING or FULLYAPPROVED override (block new requests).</summary>
        Task<bool> HasOpenOverrideAsync(int periodId);

        /// <summary>Resolves a MiscMaster row id by (MiscTypeCode, Code).</summary>
        Task<int> GetMiscMasterIdByCodeAsync(string miscTypeCode, string valueCode);

        /// <summary>
        /// Returns a small projection used by the state-machine guard and the posting gate:
        /// current StatusId, StatusCode, IsAdjustmentPeriod, FinancialYearId, CompanyId.
        /// </summary>
        Task<PeriodSnapshotDto?> GetPeriodSnapshotAsync(int periodId, CancellationToken ct);
    }

    public sealed class PeriodSnapshotDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public int StatusId { get; set; }
        public string? StatusCode { get; set; }
        public bool IsAdjustmentPeriod { get; set; }
    }
}
