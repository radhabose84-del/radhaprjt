using Contracts.Interfaces.Lookups.Purchase;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetGrnQcStatus
{
    public class GetGrnQcStatusQueryHandler : IRequestHandler<GetGrnQcStatusQuery, GrnQcStatusDto>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IGrnLookup _grnLookup;

        public GetGrnQcStatusQueryHandler(IQcInspectionQueryRepository queryRepository, IGrnLookup grnLookup)
        {
            _queryRepository = queryRepository;
            _grnLookup = grnLookup;
        }

        public async Task<GrnQcStatusDto> Handle(GetGrnQcStatusQuery request, CancellationToken cancellationToken)
        {
            var status = await _queryRepository.GetGrnStatusCountsAsync(request.GrnHeaderId);
            status.GrnHeaderId = request.GrnHeaderId;
            status.TotalLines = await _grnLookup.GetLineCountAsync(request.GrnHeaderId, cancellationToken);

            status.DerivedStatus = DeriveStatus(status);
            return status;
        }

        private static string DeriveStatus(GrnQcStatusDto s)
        {
            if (s.InspectedCount == 0)
                return "PENDING_QC";
            if (s.HoldCount > 0 || s.InspectedCount < s.TotalLines)
                return "IN_PROGRESS";
            if (s.ApprovedCount + s.ConditionallyApprovedCount == s.TotalLines && s.TotalLines > 0)
                return "COMPLETED";
            if (s.RejectedCount == s.TotalLines && s.TotalLines > 0)
                return "REJECTED";
            return "PARTIALLY_APPROVED";
        }
    }
}
