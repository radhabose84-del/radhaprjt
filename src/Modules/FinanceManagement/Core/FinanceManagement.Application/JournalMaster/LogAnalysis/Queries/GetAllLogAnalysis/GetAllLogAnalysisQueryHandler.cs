using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILogAnalysis;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetAllLogAnalysis
{
    public class GetAllLogAnalysisQueryHandler : IRequestHandler<GetAllLogAnalysisQuery, ApiResponseDTO<List<LogAnalysisDto>>>
    {
        private readonly ILogAnalysisQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllLogAnalysisQueryHandler(ILogAnalysisQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<LogAnalysisDto>>> Handle(GetAllLogAnalysisQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.LogType, request.DateFrom, request.DateTo, request.PageNumber, request.PageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllLogAnalysisQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal log analysis was fetched.",
                module: "LogAnalysis"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<LogAnalysisDto>>
            {
                IsSuccess = true,
                Message = "Log analysis retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
