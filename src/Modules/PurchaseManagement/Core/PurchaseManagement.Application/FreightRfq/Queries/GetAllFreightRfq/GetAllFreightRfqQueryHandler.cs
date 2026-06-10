using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetAllFreightRfq
{
    public class GetAllFreightRfqQueryHandler : IRequestHandler<GetAllFreightRfqQuery, ApiResponseDTO<List<FreightRfqListDto>>>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllFreightRfqQueryHandler(IFreightRfqQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FreightRfqListDto>>> Handle(GetAllFreightRfqQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.StatusId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Freight RFQ details were fetched.",
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
