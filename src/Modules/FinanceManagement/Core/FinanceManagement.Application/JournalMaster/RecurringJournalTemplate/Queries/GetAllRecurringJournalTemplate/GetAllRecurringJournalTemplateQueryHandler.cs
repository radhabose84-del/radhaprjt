using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetAllRecurringJournalTemplate
{
    public class GetAllRecurringJournalTemplateQueryHandler : IRequestHandler<GetAllRecurringJournalTemplateQuery, ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRecurringJournalTemplateQueryHandler(IRecurringJournalTemplateQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>> Handle(GetAllRecurringJournalTemplateQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<RecurringJournalTemplateHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRecurringJournalTemplateQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Recurring journal template details were fetched.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>
            {
                IsSuccess = true,
                Message = "Recurring journal template list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
