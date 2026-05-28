using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationDashboard
{
    public class GetVendorEvaluationDashboardQueryHandler : IRequestHandler<GetVendorEvaluationDashboardQuery, VendorEvaluationDashboardDto?>
    {
        private readonly IVendorEvaluationDashboardQueryRepository _dashboardRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorEvaluationDashboardQueryHandler(
            IVendorEvaluationDashboardQueryRepository dashboardRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _dashboardRepository = dashboardRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VendorEvaluationDashboardDto?> Handle(GetVendorEvaluationDashboardQuery request, CancellationToken cancellationToken)
        {
            var result = await _dashboardRepository.GetDashboardAsync(
                request.VendorId,
                request.EvaluationMonth,
                request.EvaluationYear,
                request.LookbackMonths);

            if (result == null) return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDashboard",
                actionCode: "GetVendorEvaluationDashboardQuery",
                actionName: request.VendorId.ToString(),
                details: $"Vendor Evaluation Dashboard for VendorId {request.VendorId} ({request.EvaluationMonth}/{request.EvaluationYear}) was fetched.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
