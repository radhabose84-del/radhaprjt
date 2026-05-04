using Contracts.Common;
using MediatR;
using SalesManagement.Application.Reports.SalesProjection.Dto;

namespace SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection
{
    public enum ProjectionPeriodType
    {
        Monthly = 1,
        Quarterly = 2,
        Yearly = 3
    }

    public class GetSalesProjectionQuery : IRequest<ApiResponseDTO<SalesProjectionDto>>
    {
        public ProjectionPeriodType PeriodType { get; set; } = ProjectionPeriodType.Monthly;
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
