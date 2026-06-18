using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetAllSection
{
    public class GetAllSectionQueryHandler : IRequestHandler<GetAllSectionQuery, ApiResponseDTO<List<ScheduleIIISectionDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSectionQueryHandler(IScheduleIIIQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ScheduleIIISectionDto>>> Handle(GetAllSectionQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllSectionAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.ScheduleIIIMasterId);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllSection", actionCode: "Get", actionName: data.Count.ToString(),
                details: "Schedule III sections were fetched.", module: "ScheduleIIISection"), cancellationToken);

            return new ApiResponseDTO<List<ScheduleIIISectionDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }
}
