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
            int vendorId, int evaluationMonth, int evaluationYear)
        {
            // Resolve vendor name directly — not restricted by active/unit filters
            // so the dashboard works for any vendor that has evaluation data
            var vendor = await GetVendorInfoAsync(vendorId);
            if (vendor == null) return null;

            // Single month date range
            var startDate = new DateTimeOffset(evaluationYear, evaluationMonth, 1, 0, 0, 0, TimeSpan.Zero);
            var endDate = startDate.AddMonths(1); // first day of next month

            // 1. Fetch all active criteria
            var criteria = await GetActiveCriteriaAsync();

            // 2. Calculate auto-scores for each criterion
            //    Use CalculationType if available, otherwise fall back to CriteriaCode
            foreach (var c in criteria)
            {
                var calcKey = !string.IsNullOrEmpty(c.CalculationType)
                    ? c.CalculationType
                    : c.CriteriaName;

                if (!string.IsNullOrEmpty(calcKey))
                {
                    c.IsAutoCalculated = true;
                    c.AutoCalculatedScore = await CalculateAutoScoreAsync(
                        calcKey, vendorId, startDate, endDate);
                }
            }

            // 3. Calculate total weighted score
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
                VendorName = vendor.VendorName,
                EvaluationMonth = evaluationMonth,
                EvaluationYear = evaluationYear,
                Criteria = criteria,
                TotalWeightedScore = Math.Round(totalWeightedScore, 2),
                ResolvedGradeId = resolvedGrade?.Id,
                ResolvedGradeCode = resolvedGrade?.GradeCode,
                ResolvedGradeName = resolvedGrade?.GradeName,
                GradeReferences = grades
            };
        }

        private async Task<SupplierInfo?> GetVendorInfoAsync(int vendorId)
        {
            const string sql = @"
                SELECT a.PartyCode AS VendorCode,
                       a.PartyName AS VendorName
                FROM Party.PartyMaster a
                WHERE a.Id = @VendorId AND a.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<SupplierInfo>(
                sql, new { VendorId = vendorId });
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
            string calcKey, int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var key = calcKey.ToUpperInvariant().Trim();

            // Match both CalculationType values (e.g., "DELIVERY") and CriteriaCode values (e.g., "DEL")
            if (key.Contains("DELIVERY") )
                return await CalculateDeliveryScoreAsync(vendorId, startDate, endDate);

            if (key.Contains("QC") || key.Contains("DEFECT") || key.Contains("QUALITY"))
                return await CalculateQcDefectScoreAsync(vendorId, startDate, endDate);

            if (key.Contains("QTY") || key.Contains("QUANTITY") || key.Contains("COMPLIANCE"))
                return await CalculateQtyComplianceScoreAsync(vendorId, startDate, endDate);

            if (key.Contains("RATE") || key.Contains("STABILITY") || key.Contains("PRICE"))
                return await CalculateRateStabilityScoreAsync(vendorId, startDate, endDate);

            return 0m;
        }

        private async Task<decimal> CalculateDeliveryScoreAsync(
            int vendorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            // GrnHeader/GrnDetail do NOT extend BaseEntity (no IsDeleted column)
            // PurchaseOrderHeader/PurchaseLocalHeader/PurchaseLocalDetail DO extend BaseEntity
            const string sql = @"
                WITH GrnDeliveries AS (
                    SELECT gh.Id AS GrnHeaderId,
                           gh.GrnDate,
                           pld.ScheduleDate,
                           DATEDIFF(DAY, pld.ScheduleDate, gh.GrnDate) AS DelayDays
                    FROM Purchase.GrnHeader gh
                    INNER JOIN Purchase.GrnDetail gd ON gd.GrnId = gh.Id
                    INNER JOIN Purchase.PurchaseOrderHeader poh ON gd.PoId = poh.Id
                        AND poh.IsDeleted = 0
                    INNER JOIN Purchase.PurchaseLocalHeader plh ON plh.PurchaseOrderId = poh.Id
                        AND plh.IsDeleted = 0
                    INNER JOIN Purchase.PurchaseLocalDetail pld ON pld.PurchaseLocalId = plh.Id
                        AND pld.ItemId = gd.ItemId
                        AND pld.IsDeleted = 0
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
                        AND poh.IsDeleted = 0
                    INNER JOIN Purchase.PurchaseLocalHeader plh ON plh.PurchaseOrderId = poh.Id
                        AND plh.IsDeleted = 0
                    INNER JOIN Purchase.PurchaseLocalDetail pld ON pld.PurchaseLocalId = plh.Id
                        AND pld.ItemId = gd.ItemId
                        AND pld.IsDeleted = 0
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

        // ═══════════════════════════════════════════════════════════════
        // GetAll Dashboard — paginated vendor rating summary with grade filter
        // ═══════════════════════════════════════════════════════════════

        public async Task<(List<VendorRatingDashboardListItemDto>, int, VendorRatingDashboardSummaryDto)> GetAllDashboardAsync(
            int pageNumber, int pageSize, string? searchTerm, string? grade)
        {
            var offset = (pageNumber - 1) * pageSize;

            // CTE: rank evaluations per vendor, pick latest (rn=1) and previous (rn=2) for trend
            const string baseCte = @"
                WITH RankedEvals AS (
                    SELECT veh.Id AS EvaluationHeaderId,
                           veh.VendorId,
                           veh.TotalWeightedScore,
                           veh.EvaluationMonth,
                           veh.EvaluationYear,
                           veh.EvaluationDate,
                           veh.GradeId,
                           vrg.GradeCode,
                           vrg.GradeName,
                           vrg.ActionDescription,
                           ROW_NUMBER() OVER (PARTITION BY veh.VendorId
                               ORDER BY veh.EvaluationYear DESC, veh.EvaluationMonth DESC) AS rn
                    FROM Purchase.VendorEvaluationHeader veh
                    LEFT JOIN Purchase.VendorRatingGrade vrg ON veh.GradeId = vrg.Id AND vrg.IsDeleted = 0
                    WHERE veh.IsDeleted = 0 AND veh.IsActive = 1
                ),
                LatestEval AS (
                    SELECT l.EvaluationHeaderId AS LatestEvaluationHeaderId,
                           l.VendorId,
                           l.TotalWeightedScore AS LatestScore,
                           l.EvaluationMonth,
                           l.EvaluationYear,
                           l.EvaluationDate AS LastEvaluatedDate,
                           l.GradeCode,
                           l.GradeName,
                           l.ActionDescription AS Action,
                           p.TotalWeightedScore AS PreviousScore
                    FROM RankedEvals l
                    LEFT JOIN RankedEvals p ON l.VendorId = p.VendorId AND p.rn = 2
                    WHERE l.rn = 1
                )";

            var whereClause = "WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(grade))
            {
                whereClause += " AND le.GradeCode = @Grade";
            }

            // Summary query (always unfiltered by grade to show total distribution)
            var summarySql = $@"{baseCte}
                SELECT COUNT(*) AS TotalVendors,
                       ISNULL(AVG(le.LatestScore), 0) AS AverageScore
                FROM LatestEval le";

            var gradeDistSql = $@"{baseCte}
                SELECT le.GradeCode, le.GradeName, COUNT(*) AS [Count]
                FROM LatestEval le
                WHERE le.GradeCode IS NOT NULL
                GROUP BY le.GradeCode, le.GradeName
                ORDER BY [Count] DESC";

            var countSql = $@"{baseCte}
                SELECT COUNT(*)
                FROM LatestEval le
                {whereClause}";

            var dataSql = $@"{baseCte}
                SELECT le.VendorId,
                       le.LatestEvaluationHeaderId,
                       le.LatestScore,
                       le.EvaluationMonth,
                       le.EvaluationYear,
                       le.LastEvaluatedDate,
                       le.GradeCode,
                       le.GradeName,
                       le.Action,
                       CASE
                           WHEN le.PreviousScore IS NULL THEN 'Stable'
                           WHEN le.LatestScore > le.PreviousScore THEN 'Up'
                           WHEN le.LatestScore < le.PreviousScore THEN 'Down'
                           ELSE 'Stable'
                       END AS Trend,
                       CASE
                           WHEN le.PreviousScore IS NULL OR le.PreviousScore = 0 THEN NULL
                           ELSE CAST(((le.LatestScore - le.PreviousScore) / le.PreviousScore) * 100 AS DECIMAL(18,2))
                       END AS TrendPercentage
                FROM LatestEval le
                {whereClause}
                ORDER BY le.LatestScore DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Grade = grade,
                Offset = offset,
                PageSize = pageSize
            };

            // Execute summary, grade distribution, count, and data queries
            var summaryRow = await _dbConnection.QueryFirstOrDefaultAsync<(int TotalVendors, decimal AverageScore)>(summarySql);
            var gradeDistribution = (await _dbConnection.QueryAsync<GradeCountDto>(gradeDistSql)).ToList();
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<VendorRatingDashboardListItemDto>(dataSql, parameters)).ToList();

            // Fetch ALL criteria scores from VendorEvaluationDetail for each vendor's latest evaluation
            if (data.Count > 0)
            {
                var headerIds = data.Where(d => d.LatestEvaluationHeaderId > 0)
                    .Select(d => d.LatestEvaluationHeaderId).Distinct().ToList();

                if (headerIds.Count > 0)
                {
                    const string criteriaScoreSql = @"
                        SELECT ved.VendorEvaluationHeaderId AS EvaluationHeaderId,
                               ved.CriteriaId,
                               vec.CriteriaCode,
                               vec.CriteriaName,
                               ved.Score,
                               ved.WeightagePercent,
                               ved.WeightedScore
                        FROM Purchase.VendorEvaluationDetail ved
                        INNER JOIN Purchase.VendorEvaluationCriteria vec ON ved.CriteriaId = vec.Id AND vec.IsDeleted = 0
                        WHERE ved.VendorEvaluationHeaderId IN @HeaderIds
                          AND ved.IsDeleted = 0
                        ORDER BY vec.SortOrder ASC";

                    var allScores = (await _dbConnection.QueryAsync<DetailWithHeaderId>(
                        criteriaScoreSql, new { HeaderIds = headerIds })).ToList();

                    var scoresByHeader = allScores.GroupBy(c => c.EvaluationHeaderId)
                        .ToDictionary(g => g.Key, g => g.Select(d => new CriteriaScoreItemDto
                        {
                            CriteriaId = d.CriteriaId,
                            CriteriaCode = d.CriteriaCode,
                            CriteriaName = d.CriteriaName,
                            Score = d.Score,
                            WeightagePercent = d.WeightagePercent,
                            WeightedScore = d.WeightedScore
                        }).ToList());

                    foreach (var item in data)
                    {
                        item.CriteriaScores = scoresByHeader.TryGetValue(item.LatestEvaluationHeaderId, out var scores)
                            ? scores
                            : new List<CriteriaScoreItemDto>();
                    }
                }
            }

            // Populate VendorCode / VendorName via cross-module lookup
            var vendorIds = data.Select(d => d.VendorId).Distinct().ToList();
            if (vendorIds.Count > 0)
            {
                var vendorDict = new Dictionary<int, SupplierInfo>();
                foreach (var vendorId in vendorIds)
                {
                    var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId);
                    if (supplier != null)
                    {
                        vendorDict[vendorId] = new SupplierInfo { VendorCode = supplier.VendorCode, VendorName = supplier.VendorName };
                    }
                }

                foreach (var item in data)
                {
                    if (vendorDict.TryGetValue(item.VendorId, out var info))
                    {
                        item.VendorCode = info.VendorCode;
                        item.VendorName = info.VendorName;
                    }
                }

                // Apply search term filter on vendor name/code (resolved after lookup)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    data = data.Where(d =>
                        (d.VendorName != null && d.VendorName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (d.VendorCode != null && d.VendorCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                    totalCount = data.Count;
                }
            }

            var summary = new VendorRatingDashboardSummaryDto
            {
                TotalVendors = summaryRow.TotalVendors,
                AverageScore = Math.Round(summaryRow.AverageScore, 2),
                GradeDistribution = gradeDistribution
            };

            return (data, totalCount, summary);
        }

        // ═══════════════════════════════════════════════════════════════
        // Evaluation History — full history for a specific vendor
        // ═══════════════════════════════════════════════════════════════

        public async Task<VendorEvaluationHistoryDto?> GetEvaluationHistoryAsync(int vendorId)
        {
            // Resolve vendor info via cross-module lookup
            var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId);
            if (supplier == null) return null;

            // Fetch all evaluations for this vendor (most recent first)
            const string headerSql = @"
                SELECT veh.Id AS EvaluationHeaderId,
                       veh.EvaluationCode,
                       veh.EvaluationMonth,
                       veh.EvaluationYear,
                       veh.EvaluationDate,
                       veh.TotalWeightedScore,
                       vrg.GradeCode,
                       vrg.GradeName,
                       mm.Description AS StatusName,
                       veh.CreatedByName AS EvaluatedBy,
                       veh.CreatedDate AS EvaluatedDate
                FROM Purchase.VendorEvaluationHeader veh
                LEFT JOIN Purchase.VendorRatingGrade vrg ON veh.GradeId = vrg.Id AND vrg.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster mm ON veh.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE veh.VendorId = @VendorId AND veh.IsDeleted = 0
                ORDER BY veh.EvaluationYear DESC, veh.EvaluationMonth DESC";

            var headers = (await _dbConnection.QueryAsync<EvaluationHistoryItemDto>(
                headerSql, new { VendorId = vendorId })).ToList();

            if (headers.Count == 0)
            {
                return new VendorEvaluationHistoryDto
                {
                    VendorId = vendorId,
                    VendorCode = supplier.VendorCode,
                    VendorName = supplier.VendorName,
                    CurrentScore = 0,
                    EvaluationHistory = new List<EvaluationHistoryItemDto>()
                };
            }

            // Fetch all detail rows for these evaluation headers in a single query
            var headerIds = headers.Select(h => h.EvaluationHeaderId).ToList();

            const string detailSql = @"
                SELECT ved.VendorEvaluationHeaderId AS EvaluationHeaderId,
                       ved.CriteriaId,
                       vec.CriteriaCode,
                       vec.CriteriaName,
                       ved.Score,
                       ved.WeightagePercent,
                       ved.WeightedScore
                FROM Purchase.VendorEvaluationDetail ved
                LEFT JOIN Purchase.VendorEvaluationCriteria vec ON ved.CriteriaId = vec.Id AND vec.IsDeleted = 0
                WHERE ved.VendorEvaluationHeaderId IN @HeaderIds AND ved.IsDeleted = 0
                ORDER BY ved.Id ASC";

            var allDetails = (await _dbConnection.QueryAsync<DetailWithHeaderId>(
                detailSql, new { HeaderIds = headerIds })).ToList();

            // Group details by header
            var detailsByHeader = allDetails.GroupBy(d => d.EvaluationHeaderId)
                .ToDictionary(g => g.Key, g => g.Select(d => new CriteriaScoreItemDto
                {
                    CriteriaId = d.CriteriaId,
                    CriteriaCode = d.CriteriaCode,
                    CriteriaName = d.CriteriaName,
                    Score = d.Score,
                    WeightagePercent = d.WeightagePercent,
                    WeightedScore = d.WeightedScore
                }).ToList());

            foreach (var header in headers)
            {
                header.CriteriaScores = detailsByHeader.TryGetValue(header.EvaluationHeaderId, out var scores)
                    ? scores
                    : new List<CriteriaScoreItemDto>();
            }

            var latest = headers[0];

            return new VendorEvaluationHistoryDto
            {
                VendorId = vendorId,
                VendorCode = supplier.VendorCode,
                VendorName = supplier.VendorName,
                CurrentScore = latest.TotalWeightedScore,
                CurrentGradeCode = latest.GradeCode,
                CurrentGradeName = latest.GradeName,
                EvaluationHistory = headers
            };
        }

        // Internal projection for detail query that includes the header FK
        private class DetailWithHeaderId
        {
            public int EvaluationHeaderId { get; set; }
            public int CriteriaId { get; set; }
            public string? CriteriaCode { get; set; }
            public string? CriteriaName { get; set; }
            public decimal Score { get; set; }
            public decimal WeightagePercent { get; set; }
            public decimal WeightedScore { get; set; }
        }

        private class SupplierInfo
        {
            public string? VendorCode { get; set; }
            public string? VendorName { get; set; }
        }
    }
}
