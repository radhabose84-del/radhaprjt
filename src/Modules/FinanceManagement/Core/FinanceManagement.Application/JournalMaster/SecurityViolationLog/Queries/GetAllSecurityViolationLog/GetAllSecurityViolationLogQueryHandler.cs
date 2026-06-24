using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ISecurityViolationLog;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.SecurityViolationLog.Queries.GetAllSecurityViolationLog
{
    public class GetAllSecurityViolationLogQueryHandler : IRequestHandler<GetAllSecurityViolationLogQuery, ApiResponseDTO<List<SecurityViolationLogDto>>>
    {
        private readonly ISecurityViolationLogQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSecurityViolationLogQueryHandler(ISecurityViolationLogQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SecurityViolationLogDto>>> Handle(GetAllSecurityViolationLogQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.JournalHeaderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSecurityViolationLogQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Security violation log details were fetched.",
                module: "SecurityViolationLog"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SecurityViolationLogDto>>
            {
                IsSuccess = true,
                Message = "Security violation log retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
