using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Reports.AssetAudit
{
    public class AssetAuditReportDto
    {
          // From Audit
        public string? Audit_UnitName { get; set; }
        public string? Audit_AssetCode { get; set; }
        public string? Audit_AssetName { get; set; }
        public string? Audit_Department { get; set; }
        public string? AuditorName { get; set; }
        public DateTimeOffset? AuditDate { get; set; }
        public string? SourceFileName { get; set; }
        public int Audit_UnitId { get; set; }
        public string? ScanType { get; set; }
        public string? AuditFinancialYear { get; set; }

        // Comparison Result
        public string? ComparisonStatus { get; set; }
        public string? UnitChange { get; set; }
        public string? DepartmentChange { get; set; }

        // From Master
        public string? Book_AssetCode { get; set; }
        public string? Book_AssetName { get; set; }
        public string? Book_Department { get; set; }
        public string? Book_UnitName { get; set; }
    }
}