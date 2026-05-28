using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader
{
    public class VendorEvaluationDashboardQueryRepository : IVendorEvaluationDashboardQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ISupplierLookup _supplierLookup;

        public VendorEvaluationDashboardQueryRepository(IDbConnection dbConnection, ISupplierLookup supplierLookup)
        {
            _dbConnection = dbConnection;
            _supplierLookup = supplierLookup;
        }

        public async Task<VendorEvaluationDashboardDto?> GetDashboardAsync(
            int vendorId, int evaluationMonth, int evaluationYear, int lookbackMonths)
        {
            // Resolve vendor name via cross-module lookup
            var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId);
            if (supplier == null) return null;

            // Calculate lookback date range
            var endDate = new DateTimeOffset(evaluationYear, evaluationMonth, 1, 0, 0, 0, TimeSpan.Zero)
                .AddMonths(1); // first day of next month
            var startDate = endDate.AddMonths(-lookbackMonths);

            // 1. Fetch all active criteria
            var criteria = await GetActiveCriteriaAsync();

            // 2. Calculate auto-scores for each criterion
            foreach (var c in criteria)
            {
                if (!string.IsNullOrEmpty(c.CalculationType))
                {
                    c.IsAutoCalculated = true;
                    c.AutoCalculatedScore = await CalculateAutoScoreAsync(
                        c.CalculationType, vendorId, startDate, endDate);
                }
            }

            // 3. Calculate total weighted score (auto-calculated criteria only)
            decimal totalWeightedScore = 0;
            foreach (var c in criteria)
            {
                if (c.IsAutoCalculated && c.AutoCalculatedScore.HasValue)
                {
                    totalWeightedScore += (c.AutoCalculatedScore.Value * c.WeightagePercent) / 100m;
                }
            }

            // 4. Fetch grade references and resolve grade
            var grades = await GetGradeReferencesAsync();
            GradeReferenceDto? resolvedGrade = null;
            foreach (var g in grades)
            {
                if (totalWeightedScore >= g.MinScore && totalWeightedScore <= g.MaxScore)
                {
                    resolvedGrade = g;
                    break;
                }
            }

            return new VendorEvaluationDashboardDto
            {
                VendorId = vendorId,
                VendorName = supplier.VendorName,
                EvaluationMonth = evaluationMonth,
                EvaluationYear = evaluationYear,
                LookbackMonths = lookbackMonths,
                Criteria = criteria,
                TotalWeightedScore = Math.Round(totalWeightedScore, 2),
                ResolvedGradeId = resolvedGrade?.Id,
                ResolvedGradeCode = resolvedGrade?.GradeCode,
                ResolvedGradeName = resolvedGrade?.GradeName,
                GradeReferences = grades
            };
        }

        private async Task<List<DashboardCriteriaDto>> GetActiveCriteriaAsync()
        {
            const string sql = @"
                SELECT vec.Id AS CriteriaId, vec.CriteriaCode, vec.CriteriaName,
                       vec.Description, vec.WeightagePercent, vec.MinimumScore,
                       vec.CalculationType,
                       sm.Description AS ScoringMethodName
                FROM Purchase.VendorEvaluationCriteria vec
                LEFT JOIN Purchase.MiscMaster sm ON vec.ScoringMethodId = sm.Id AND sm.IsDeleted = 0
                WHERE vec.IsActive = 1 AND vec.IsDeleted = 0
                ORDER BY vec.SortOrder ASC";

            var result = await _dbConnection.QueryAsync<DashboardCriteriaDto>(sql);
            return result.ToList();
        }

        private async Task<decimal> CalculateAutoScoreAsync(
            string calculationType, int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return calculationType.ToUpperInvariant() switch
            {
                "DELIVERY" => await CalculateDeliveryScoreAsync(vendorId, startDate, endDate),
                "QC_DEFECT" => await CalculateQcDefectScoreAsync(vendorId, startDate, endDate),
                "QTY_COMPLIANCE" => await CalculateQtyComplianceScoreAsync(vendorId, startDate, endDate),
                "RATE_STABILITY" => await CalculateRateStabilityScoreAsync(vendorId, startDate, endDate),
                _ => 0m
            };
        }

        private async Task<decimal> CalculateDeliveryScoreAsync(
            int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            const string sql = @"
                WITH GrnDeliveries AS (
                    SELECT gh.Id AS GrnHeaderId,
                           gh.GrnDate,
                           pld.ScheduleDate,
                           DATEDIFF(DAY, pld.ScheduleDate, gh.GrnDate) AS DelayDays
                    FROM Purchase.GrnHeader gh
                    INNER JOIN Purchase.GrnDetail gd ON gd.GrnId = gh.Id
                    INNER JOIN Purchase.PurchaseOrderHeader poh ON gd.PoId = poh.Id
                    INNER JOIN Purchase.PurchaseLocalHeader plh ON plh.PurchaseOrderId = poh.Id
                    INNER JOIN Purchase.PurchaseLocalDetail pld ON pld.PurchaseLocalId = plh.Id
                              AND pld.ItemId = gd.ItemId
                    WHERE gh.PartyId = @VendorId
                      AND pld.ScheduleDate IS NOT NULL
                      AND gh.GrnDate >= @StartDate AND gh.GrnDate < @EndDate
                )
                SELECT ISNULL(AVG(CAST(dsr.Score AS DECIMAL(18,2))), 0) AS Score
                FROM GrnDeliveries gd
                LEFT JOIN Purchase.DeliveryScoreRule dsr
                    ON gd.DelayDays >= dsr.DelayDaysFrom
                    AND gd.DelayDays <= dsr.DelayDaysTo
                    AND dsr.IsActive = 1 AND dsr.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new
            {
                VendorId = vendorId,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        private async Task<decimal> CalculateQcDefectScoreAsync(
            int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            const string sql = @"
                SELECT CASE WHEN SUM(gd.ReceivedQuantity) > 0
                    THEN CAST((1.0 - (SUM(ISNULL(gd.QcRejectedQuantity, 0)) / SUM(gd.ReceivedQuantity))) * 100 AS DECIMAL(18,2))
                    ELSE 0 END AS Score
                FROM Purchase.GrnHeader gh
                INNER JOIN Purchase.GrnDetail gd ON gd.GrnId = gh.Id
                WHERE gh.PartyId = @VendorId
                  AND gh.GrnDate >= @StartDate AND gh.GrnDate < @EndDate
                  AND gh.IsQcApproved = 1";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new
            {
                VendorId = vendorId,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        private async Task<decimal> CalculateQtyComplianceScoreAsync(
            int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            const string sql = @"
                SELECT ISNULL(AVG(
                    CASE WHEN gd.OrderQuantity > 0
                         THEN CASE WHEN gd.ReceivedQuantity / gd.OrderQuantity > 1
                              THEN CAST(1.0 AS DECIMAL(18,4))
                              ELSE CAST(gd.ReceivedQuantity AS DECIMAL(18,4)) / CAST(gd.OrderQuantity AS DECIMAL(18,4)) END
                         ELSE 0 END
                ) * 100, 0) AS Score
                FROM Purchase.GrnHeader gh
                INNER JOIN Purchase.GrnDetail gd ON gd.GrnId = gh.Id
                WHERE gh.PartyId = @VendorId
                  AND gh.GrnDate >= @StartDate AND gh.GrnDate < @EndDate";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new
            {
                VendorId = vendorId,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        private async Task<decimal> CalculateRateStabilityScoreAsync(
            int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            const string sql = @"
                WITH PriceChanges AS (
                    SELECT gd.ItemId,
                           ABS(CASE WHEN pld.UnitPrice > 0
                               THEN ((ISNULL(gd.UnitPrice, pld.UnitPrice) - pld.UnitPrice) / pld.UnitPrice) * 100
                               ELSE 0 END) AS PctChange
                    FROM Purchase.GrnHeader gh
                    INNER JOIN Purchase.GrnDetail gd ON gd.GrnId = gh.Id
                    INNER JOIN Purchase.PurchaseOrderHeader poh ON gd.PoId = poh.Id
                    INNER JOIN Purchase.PurchaseLocalHeader plh ON plh.PurchaseOrderId = poh.Id
                    INNER JOIN Purchase.PurchaseLocalDetail pld ON pld.PurchaseLocalId = plh.Id
                              AND pld.ItemId = gd.ItemId
                    WHERE gh.PartyId = @VendorId
                      AND gh.GrnDate >= @StartDate AND gh.GrnDate < @EndDate
                )
                SELECT CASE WHEN (100.0 - ISNULL(AVG(CAST(PctChange AS DECIMAL(18,2))), 0)) < 0 THEN 0
                       ELSE CAST(100.0 - ISNULL(AVG(CAST(PctChange AS DECIMAL(18,2))), 0) AS DECIMAL(18,2)) END AS Score
                FROM PriceChanges";

            return await _dbConnection.ExecuteScalarAsync<decimal>(sql, new
            {
                VendorId = vendorId,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        private async Task<List<GradeReferenceDto>> GetGradeReferencesAsync()
        {
            const string sql = @"
                SELECT Id, GradeCode, GradeName, MinScore, MaxScore, ActionDescription
                FROM Purchase.VendorRatingGrade
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY MinScore ASC";

            var result = await _dbConnection.QueryAsync<GradeReferenceDto>(sql);
            return result.ToList();
        }
    }
}
