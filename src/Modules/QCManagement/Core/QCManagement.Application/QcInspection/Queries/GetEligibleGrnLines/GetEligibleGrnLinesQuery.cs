using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetEligibleGrnLines
{
    public class GetEligibleGrnLinesQuery : IRequest<IReadOnlyList<EligibleGrnLineDto>>
    {
        public int? SupplierId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
