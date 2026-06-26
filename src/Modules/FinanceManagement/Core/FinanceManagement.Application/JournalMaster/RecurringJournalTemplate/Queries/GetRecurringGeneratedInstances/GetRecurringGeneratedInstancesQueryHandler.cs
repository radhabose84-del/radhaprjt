using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringGeneratedInstances
{
    public class GetRecurringGeneratedInstancesQueryHandler : IRequestHandler<GetRecurringGeneratedInstancesQuery, ApiResponseDTO<List<RecurringGeneratedInstanceDto>>>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetRecurringGeneratedInstancesQueryHandler(IRecurringJournalTemplateQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RecurringGeneratedInstanceDto>>> Handle(GetRecurringGeneratedInstancesQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetGeneratedInstancesAsync(request.PageNumber, request.PageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetRecurringGeneratedInstancesQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Recurring journal generated instances were fetched.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RecurringGeneratedInstanceDto>>
            {
                IsSuccess = true,
                Message = "Generated instances retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
