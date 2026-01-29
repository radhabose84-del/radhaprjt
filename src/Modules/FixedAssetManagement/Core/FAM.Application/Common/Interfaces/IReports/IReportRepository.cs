
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Reports.AssetAudit;
using FAM.Application.Reports.AssetReport;
using FAM.Application.Reports.AssetTransferReport;

namespace FAM.Application.Common.Interfaces.IReports
{
    public interface IReportRepository
    {
        Task<List<AssetReportDto>> AssetReportAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate);
        Task<List<AssetTransferDetailsDto>> AssetTransferReportAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate);
        Task<List<AssetAuditReportDto>> AssetAuditReportAsync( int auditCycle );
    }
}