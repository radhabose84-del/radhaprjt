using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetAllDeliveryScoreRule
{
    public class GetAllDeliveryScoreRuleQueryHandler : IRequestHandler<GetAllDeliveryScoreRuleQuery, ApiResponseDTO<List<DeliveryScoreRuleDto>>>
    {
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDeliveryScoreRuleQueryHandler(IDeliveryScoreRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DeliveryScoreRuleDto>>> Handle(GetAllDeliveryScoreRuleQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<DeliveryScoreRuleDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDeliveryScoreRuleQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "DeliveryScoreRule details were fetched.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DeliveryScoreRuleDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
