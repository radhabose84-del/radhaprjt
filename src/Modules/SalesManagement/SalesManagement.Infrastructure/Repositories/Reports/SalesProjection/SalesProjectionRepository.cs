using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesProjection;
using SalesManagement.Application.Reports.SalesProjection.Dto;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;

namespace SalesManagement.Infrastructure.Repositories.Reports.SalesProjection
{
    internal sealed class SalesProjectionRepository : ISalesProjectionRepository
    {
        private readonly IDbConnection _dbConnection;

        public SalesProjectionRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<SalesProjectionDto> GetProjectionAsync(
            ProjectionPeriodType periodType,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            CancellationToken ct = default)
        {
            var effectiveDateFrom = dateFrom ?? new DateOnly(DateTime.UtcNow.Year, 1, 1);
            var effectiveDateTo = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // SalesLead.InteractionDate is DateTimeOffset — need DateTime params
            var dateFromDt = effectiveDateFrom.ToDateTime(TimeOnly.MinValue);
            var dateToDt = effectiveDateTo.ToDateTime(TimeOnly.MaxValue);

            var leadSql = BuildLeadSql(periodType);
            var quotationSql = BuildQuotationSql(periodType);
            var orderSql = BuildOrderSql(periodType);
            var invoiceSql = BuildInvoiceSql(periodType);

            var leadParams = new { DateFrom = dateFromDt, DateTo = dateToDt };
            var dateOnlyParams = new { DateFrom = effectiveDateFrom, DateTo = effectiveDateTo };

            var leads = (await _dbConnection.QueryAsync<PeriodAggregateRow>(
                new CommandDefinition(leadSql, leadParams, cancellationToken: ct))).ToList();
            var quotations = (await _dbConnection.QueryAsync<PeriodValueRow>(
                new CommandDefinition(quotationSql, dateOnlyParams, cancellationToken: ct))).ToList();
            var orders = (await _dbConnection.QueryAsync<PeriodValueRow>(
                new CommandDefinition(orderSql, dateOnlyParams, cancellationToken: ct))).ToList();
            var invoices = (await _dbConnection.QueryAsync<PeriodValueRow>(
                new CommandDefinition(invoiceSql, dateOnlyParams, cancellationToken: ct))).ToList();

            // Build period skeleton from date range
            var periods = BuildPeriods(periodType, effectiveDateFrom, effectiveDateTo);

            // Merge query results into period DTOs
            foreach (var period in periods)
            {
                var lead = leads.FirstOrDefault(l => l.PeriodKey == period.PeriodLabel);
                if (lead != null)
                    period.LeadCount = lead.RecordCount;

                var quotation = quotations.FirstOrDefault(q => q.PeriodKey == period.PeriodLabel);
                if (quotation != null)
                {
                    period.QuotationCount = quotation.RecordCount;
                    period.QuotationValue = quotation.TotalValue;
                }

                var order = orders.FirstOrDefault(o => o.PeriodKey == period.PeriodLabel);
                if (order != null)
                {
                    period.OrderCount = order.RecordCount;
                    period.OrderValue = order.TotalValue;
                }

                var invoice = invoices.FirstOrDefault(i => i.PeriodKey == period.PeriodLabel);
                if (invoice != null)
                {
                    period.InvoicedCount = invoice.RecordCount;
                    period.InvoicedValue = invoice.TotalValue;
                }

                // Weighted projection formula
                period.ProjectedRevenue =
                    (period.QuotationValue * 0.5m) +
                    (period.OrderValue * 0.9m) +
                    period.InvoicedValue;
            }

            var summary = new SalesProjectionSummaryDto
            {
                TotalLeads = periods.Sum(p => p.LeadCount),
                TotalQuotations = periods.Sum(p => p.QuotationCount),
                TotalQuotationValue = periods.Sum(p => p.QuotationValue),
                TotalOrders = periods.Sum(p => p.OrderCount),
                TotalOrderValue = periods.Sum(p => p.OrderValue),
                TotalInvoiced = periods.Sum(p => p.InvoicedCount),
                TotalInvoicedValue = periods.Sum(p => p.InvoicedValue),
                TotalProjectedRevenue = periods.Sum(p => p.ProjectedRevenue)
            };

            return new SalesProjectionDto
            {
                PeriodType = periodType.ToString(),
                DateFrom = effectiveDateFrom,
                DateTo = effectiveDateTo,
                Periods = periods,
                Summary = summary
            };
        }

        private static string BuildLeadSql(ProjectionPeriodType periodType)
        {
            var groupExpr = GetDateGroupExpression("InteractionDate", periodType);
            return $@"
                SELECT
                    {groupExpr} AS PeriodKey,
                    COUNT(*) AS RecordCount
                FROM Sales.SalesLead
                WHERE IsDeleted = 0
                  AND InteractionDate >= @DateFrom
                  AND InteractionDate <= @DateTo
                GROUP BY {groupExpr}
                ORDER BY MIN(InteractionDate);";
        }

        private static string BuildQuotationSql(ProjectionPeriodType periodType)
        {
            var groupExpr = GetDateGroupExpression("qh.QuotationDate", periodType);
            return $@"
                SELECT
                    {groupExpr} AS PeriodKey,
                    COUNT(*) AS RecordCount,
                    ISNULL(SUM(qh.GrandTotal), 0) AS TotalValue
                FROM Sales.SalesQuotationHeader qh
                LEFT JOIN Sales.MiscMaster mm ON qh.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE qh.IsDeleted = 0
                  AND qh.QuotationDate >= @DateFrom
                  AND qh.QuotationDate <= @DateTo
                  AND (mm.Description IS NULL OR mm.Description NOT IN ('Rejected', 'Cancelled'))
                  AND NOT EXISTS (
                      SELECT 1 FROM Sales.SalesOrderHeader soh
                      WHERE soh.SalesQuotationHeaderId = qh.Id
                        AND soh.IsDeleted = 0
                        AND soh.CancelledDate IS NULL
                  )
                GROUP BY {groupExpr}
                ORDER BY MIN(qh.QuotationDate);";
        }

        private static string BuildOrderSql(ProjectionPeriodType periodType)
        {
            var groupExpr = GetDateGroupExpression("soh.OrderDate", periodType);
            return $@"
                SELECT
                    {groupExpr} AS PeriodKey,
                    COUNT(*) AS RecordCount,
                    ISNULL(SUM(soh.FinalAmount), 0) AS TotalValue
                FROM Sales.SalesOrderHeader soh
                WHERE soh.IsDeleted = 0
                  AND soh.OrderDate >= @DateFrom
                  AND soh.OrderDate <= @DateTo
                  AND soh.CancelledDate IS NULL
                  AND soh.ForeClosedDate IS NULL
                GROUP BY {groupExpr}
                ORDER BY MIN(soh.OrderDate);";
        }

        private static string BuildInvoiceSql(ProjectionPeriodType periodType)
        {
            var groupExpr = GetDateGroupExpression("ih.InvoiceDate", periodType);
            return $@"
                SELECT
                    {groupExpr} AS PeriodKey,
                    COUNT(*) AS RecordCount,
                    ISNULL(SUM(ih.InvoiceAmount), 0) AS TotalValue
                FROM Sales.InvoiceHeader ih
                LEFT JOIN Sales.MiscMaster mm ON ih.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ih.IsDeleted = 0
                  AND ih.InvoiceDate >= @DateFrom
                  AND ih.InvoiceDate <= @DateTo
                  AND (mm.Description IS NULL OR mm.Description NOT IN ('Cancelled'))
                GROUP BY {groupExpr}
                ORDER BY MIN(ih.InvoiceDate);";
        }

        private static string GetDateGroupExpression(string columnName, ProjectionPeriodType periodType)
        {
            return periodType switch
            {
                ProjectionPeriodType.Monthly =>
                    $"FORMAT({columnName}, 'MMM yyyy')",
                ProjectionPeriodType.Quarterly =>
                    $"CONCAT('Q', DATEPART(QUARTER, {columnName}), ' ', YEAR({columnName}))",
                ProjectionPeriodType.Yearly =>
                    $"CAST(YEAR({columnName}) AS VARCHAR(4))",
                _ =>
                    $"FORMAT({columnName}, 'MMM yyyy')"
            };
        }

        private static List<SalesProjectionPeriodDto> BuildPeriods(
            ProjectionPeriodType periodType, DateOnly from, DateOnly to)
        {
            var periods = new List<SalesProjectionPeriodDto>();

            switch (periodType)
            {
                case ProjectionPeriodType.Monthly:
                    var current = new DateOnly(from.Year, from.Month, 1);
                    while (current <= to)
                    {
                        var endOfMonth = current.AddMonths(1).AddDays(-1);
                        periods.Add(new SalesProjectionPeriodDto
                        {
                            PeriodLabel = current.ToString("MMM yyyy"),
                            PeriodStart = current,
                            PeriodEnd = endOfMonth > to ? to : endOfMonth
                        });
                        current = current.AddMonths(1);
                    }
                    break;

                case ProjectionPeriodType.Quarterly:
                    var qStart = new DateOnly(from.Year, ((from.Month - 1) / 3) * 3 + 1, 1);
                    while (qStart <= to)
                    {
                        var quarter = (qStart.Month - 1) / 3 + 1;
                        var qEnd = qStart.AddMonths(3).AddDays(-1);
                        periods.Add(new SalesProjectionPeriodDto
                        {
                            PeriodLabel = $"Q{quarter} {qStart.Year}",
                            PeriodStart = qStart,
                            PeriodEnd = qEnd > to ? to : qEnd
                        });
                        qStart = qStart.AddMonths(3);
                    }
                    break;

                case ProjectionPeriodType.Yearly:
                    for (var year = from.Year; year <= to.Year; year++)
                    {
                        periods.Add(new SalesProjectionPeriodDto
                        {
                            PeriodLabel = year.ToString(),
                            PeriodStart = new DateOnly(year, 1, 1),
                            PeriodEnd = year == to.Year ? to : new DateOnly(year, 12, 31)
                        });
                    }
                    break;
            }

            return periods;
        }

        private sealed class PeriodAggregateRow
        {
            public string? PeriodKey { get; set; }
            public int RecordCount { get; set; }
        }

        private sealed class PeriodValueRow
        {
            public string? PeriodKey { get; set; }
            public int RecordCount { get; set; }
            public decimal TotalValue { get; set; }
        }
    }
}
