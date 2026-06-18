using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetAllLineItem
{
    public class GetAllLineItemQueryHandler : IRequestHandler<GetAllLineItemQuery, ApiResponseDTO<List<ScheduleIIISectionItemDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllLineItemQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ScheduleIIISectionItemDto>>> Handle(GetAllLineItemQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllLineItemAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.ScheduleIIIMasterId, request.SectionId);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllLineItem", actionCode: "Get", actionName: data.Count.ToString(),
                details: "Schedule III line items were fetched.", module: "ScheduleIIISectionItem"), cancellationToken);

            return new ApiResponseDTO<List<ScheduleIIISectionItemDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }
}
