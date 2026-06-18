using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportLogs
{
    public class GetGlAccountImportLogsQueryHandler
        : IRequestHandler<GetGlAccountImportLogsQuery, ApiResponseDTO<List<GlAccountImportLogDto>>>
    {
        private readonly IGlAccountImportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetGlAccountImportLogsQueryHandler(
            IGlAccountImportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GlAccountImportLogDto>>> Handle(
            GetGlAccountImportLogsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var (logs, totalCount) = await _queryRepository.GetLogsAsync(companyId, pageNumber, pageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGlAccountImportLogsQuery",
                actionName: logs.Count.ToString(),
                details: "COA import logs were fetched.",
                module: "GlAccountImport");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GlAccountImportLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
