using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHistory
{
    public class GetVendorEvaluationHistoryQueryHandler : IRequestHandler<GetVendorEvaluationHistoryQuery, VendorEvaluationHistoryDto?>
    {
        private readonly IVendorEvaluationDashboardQueryRepository _dashboardRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorEvaluationHistoryQueryHandler(
            IVendorEvaluationDashboardQueryRepository dashboardRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _dashboardRepository = dashboardRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VendorEvaluationHistoryDto?> Handle(
            GetVendorEvaluationHistoryQuery request, CancellationToken cancellationToken)
        {
            var result = await _dashboardRepository.GetEvaluationHistoryAsync(request.VendorId);

            if (result == null) return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetEvaluationHistory",
                actionCode: "GetVendorEvaluationHistoryQuery",
                actionName: request.VendorId.ToString(),
                details: $"Vendor Evaluation History for VendorId {request.VendorId} was fetched.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
