using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalFlag
{
    public class GetAllJournalFlagQueryHandler : IRequestHandler<GetAllJournalFlagQuery, ApiResponseDTO<List<JournalFlagDto>>>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllJournalFlagQueryHandler(IJournalThresholdRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalFlagDto>>> Handle(GetAllJournalFlagQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetFlagsAsync(request.PageNumber, request.PageSize, request.JournalHeaderId);

            var dtos = _mapper.Map<List<JournalFlagDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllJournalFlagQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal flag details were fetched.",
                module: "JournalFlag"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalFlagDto>>
            {
                IsSuccess = true,
                Message = "Journal flag list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
