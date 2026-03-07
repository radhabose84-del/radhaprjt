using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetAllDeliveryChallan
{
    public class GetAllDeliveryChallanQueryHandler : IRequestHandler<GetAllDeliveryChallanQuery, ApiResponseDTO<List<DeliveryChallanHeaderDto>>>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDeliveryChallanQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DeliveryChallanHeaderDto>>> Handle(GetAllDeliveryChallanQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDeliveryChallanQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Delivery Challan details were fetched.",
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
