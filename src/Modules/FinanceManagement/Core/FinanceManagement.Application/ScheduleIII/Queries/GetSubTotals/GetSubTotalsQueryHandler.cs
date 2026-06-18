using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals
{
    public class GetSubTotalsQueryHandler : IRequestHandler<GetSubTotalsQuery, ApiResponseDTO<List<ScheduleIIISubTotalDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSubTotalsQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ScheduleIIISubTotalDto>>> Handle(GetSubTotalsQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetSubTotalsAsync();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSubTotals",
                actionCode: "GetSubTotalsQuery",
                actionName: data.Count.ToString(),
                details: "Schedule III sub-totals were fetched.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ScheduleIIISubTotalDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
