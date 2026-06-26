using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class PeriodStatusOverrideBuilders
    {
        public static PeriodSnapshotDto OpenPeriodSnapshot(int periodId = 1, int companyId = 1) =>
            new()
            {
                Id = periodId,
                CompanyId = companyId,
                FinancialYearId = 1,
                StatusId = 100,
                StatusCode = "OPEN",
                IsAdjustmentPeriod = false
            };

        public static PeriodSnapshotDto SoftClosedPeriodSnapshot(int periodId = 1, int companyId = 1) =>
            new()
            {
                Id = periodId,
                CompanyId = companyId,
                FinancialYearId = 1,
                StatusId = 200,
                StatusCode = "SOFTCLOSED",
                IsAdjustmentPeriod = false
            };

        public static PeriodSnapshotDto HardClosedPeriodSnapshot(int periodId = 1, int companyId = 1) =>
            new()
            {
                Id = periodId,
                CompanyId = companyId,
                FinancialYearId = 1,
                StatusId = 300,
                StatusCode = "HARDCLOSED",
                IsAdjustmentPeriod = false
            };

        public static PeriodSnapshotDto AdjustmentPeriodSnapshot(int periodId = 1, int companyId = 1) =>
            new()
            {
                Id = periodId,
                CompanyId = companyId,
                FinancialYearId = 1,
                StatusId = 100,
                StatusCode = "OPEN",
                IsAdjustmentPeriod = true
            };

        public static PeriodStatusOverrideDto PendingOverrideDto(
            int id = 1,
            int periodId = 1,
            int requestedBy = 42,
            string fromCode = "HARDCLOSED",
            string toCode = "SOFTCLOSED",
            string statusCode = "PENDING",
            int? cfoApproverId = null,
            int? sysAdminApproverId = null) =>
            new()
            {
                Id = id,
                AccountingPeriodId = periodId,
                CompanyId = 1,
                FromStatusId = 300,
                FromStatusCode = fromCode,
                ToStatusId = 200,
                ToStatusCode = toCode,
                RequestedBy = requestedBy,
                RequestedAt = DateTimeOffset.UtcNow,
                RequestedReason = "Audit correction",
                CfoApproverId = cfoApproverId,
                CfoApprovedAt = cfoApproverId.HasValue ? DateTimeOffset.UtcNow : null,
                SysAdminApproverId = sysAdminApproverId,
                SysAdminApprovedAt = sysAdminApproverId.HasValue ? DateTimeOffset.UtcNow : null,
                OverrideStatusId = 400,
                OverrideStatusCode = statusCode
            };
    }
}
