using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRules
{
    public class GetPutAwayRulesQueryHandler : IRequestHandler<GetPutAwayRulesQuery, ApiResponseDTO<List<PutAwayRuleListDto>>>
    {
        private readonly IPutAwayRuleQueryRepository _queryRepo;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetPutAwayRulesQueryHandler(IPutAwayRuleQueryRepository queryRepo, IMediator mediator, IMapper mapper)
        {
            _queryRepo = queryRepo;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<PutAwayRuleListDto>>> Handle(GetPutAwayRulesQuery request, CancellationToken cancellationToken)
        {
            var (rows, total) = await _queryRepo.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken);
            var dto = _mapper.Map<List<PutAwayRuleListDto>>(rows);

            // 📘 Audit
            var ev = new AuditLogsDomainEvent(
                actionDetail: "GetPutAwayRules",
                actionCode: "Get",
                actionName: dto.Count.ToString(),
                details: "Put-away rules were fetched.",
                module: "PutAway"
            );
            await _mediator.Publish(ev, cancellationToken);

            return new ApiResponseDTO<List<PutAwayRuleListDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dto,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
