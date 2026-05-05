using SalesManagement.Application.Reports.SalesProjection.Dto;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;

namespace SalesManagement.Application.Common.Interfaces.ISalesProjection
{
    public interface ISalesProjectionRepository
    {
        Task<SalesProjectionDto> GetProjectionAsync(
            ProjectionPeriodType periodType,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            CancellationToken ct = default);
    }
}
