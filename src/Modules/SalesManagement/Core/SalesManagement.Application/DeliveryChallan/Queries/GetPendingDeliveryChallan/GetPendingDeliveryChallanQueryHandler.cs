using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan
{
    public class GetPendingDeliveryChallanQueryHandler : IRequestHandler<GetPendingDeliveryChallanQuery, ApiResponseDTO<List<DeliveryChallanHeaderDto>>>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPendingDeliveryChallanQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DeliveryChallanHeaderDto>>> Handle(GetPendingDeliveryChallanQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingDeliveryChallan",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending Delivery Challan details were fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DeliveryChallanHeaderDto>>
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
