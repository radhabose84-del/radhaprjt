using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPending
{
    public class GetFreightRfqPendingQueryHandler : IRequestHandler<GetFreightRfqPendingQuery, ApiResponseDTO<List<FreightRfqListDto>>>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetFreightRfqPendingQueryHandler(IFreightRfqQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FreightRfqListDto>>> Handle(GetFreightRfqPendingQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingAsync(request.PageNumber, request.PageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFreightRfqPendingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending Freight RFQ details were fetched.",
                module: "FreightRfq"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FreightRfqListDto>>
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
