using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IServiceHistory;
using MaintenanceManagement.Application.ServiceHistory.Dto;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory
{
    public class GetServiceHistoryQueryHandler
        : IRequestHandler<GetServiceHistoryQuery, ApiResponseDTO<List<ServiceHistoryDto>>>
    {
        private readonly IServiceHistoryQueryRepository _serviceHistoryQueryRepository;
        private readonly IMediator _mediator;

        public GetServiceHistoryQueryHandler(
            IServiceHistoryQueryRepository serviceHistoryQueryRepository,
            IMediator mediator)
        {
            _serviceHistoryQueryRepository = serviceHistoryQueryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ServiceHistoryDto>>> Handle(
            GetServiceHistoryQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _serviceHistoryQueryRepository.GetServiceHistoryAsync(
                request.MachineId,
                request.AssetId,
                request.FromDate,
                request.ToDate,
                request.PageNumber,
                request.PageSize);

            // Domain Event (audit)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "SERVICEHISTORY_GET",
                actionName: items.Count.ToString(),
                details: "Service history was fetched.",
                module: "ServiceHistory"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ServiceHistoryDto>>
            {
                IsSuccess = true,
                Message = items.Count > 0 ? "Success" : "No service history found.",
                Data = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
