using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetAllArrival
{
    public class GetAllArrivalQueryHandler : IRequestHandler<GetAllArrivalQuery, ApiResponseDTO<List<ArrivalDto>>>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllArrivalQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ArrivalDto>>> Handle(GetAllArrivalQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.PendingStatus);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllArrivalQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Arrival details were fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ArrivalDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
