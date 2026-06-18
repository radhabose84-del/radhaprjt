using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Commands.ActivateGlAccountImportBatch
{
    /// <summary>
    /// Bulk "Activate" for an import batch (AC3): flips every still-inactive account created by the
    /// given import run to Active in one call.
    /// </summary>
    public class ActivateGlAccountImportBatchCommandHandler
        : IRequestHandler<ActivateGlAccountImportBatchCommand, ApiResponseDTO<int>>
    {
        private readonly IGlAccountImportCommandRepository _commandRepository;
        private readonly IGlAccountImportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public ActivateGlAccountImportBatchCommandHandler(
            IGlAccountImportCommandRepository commandRepository,
            IGlAccountImportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            ActivateGlAccountImportBatchCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            if (request.ImportLogId <= 0 ||
                !await _queryRepository.LogBelongsToCompanyAsync(request.ImportLogId, companyId))
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Import batch not found.",
                    Data = 0
                };
            }

            var activated = await _commandRepository.ActivateBatchAsync(request.ImportLogId, companyId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Activate",
                actionCode: "GL_ACCOUNT_IMPORT_ACTIVATE",
                actionName: request.ImportLogId.ToString(),
                details: $"Activated {activated} account(s) from import batch {request.ImportLogId} (Company {companyId}).",
                module: "GlAccountImport");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{activated} account(s) activated.",
                Data = activated
            };
        }
    }
}
