using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre
{
    public class CreateProfitCentreCommandHandler : IRequestHandler<CreateProfitCentreCommand, ApiResponseDTO<int>>
    {
        private readonly IProfitCentreCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateProfitCentreCommandHandler(
            IProfitCentreCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateProfitCentreCommand request, CancellationToken cancellationToken)
        {
            // Owning company — resolved from the JWT, never the payload. Code uniqueness is global
            // (across companies), so this is stored for audit only.
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.ProfitCentre>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PROFIT_CENTRE_CREATE",
                actionName: request.ProfitCentreCode ?? string.Empty,
                details: $"Profit Centre '{request.ProfitCentreCode}' ({request.ProfitCentreName}) created successfully with Id {newId}.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // AC#4 — mid-year addition: record that prior transactions cannot be retro-tagged.
            if (!string.IsNullOrWhiteSpace(request.MidYearJustification))
            {
                var midYearNote = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: "PROFIT_CENTRE_MIDYEAR_ADD",
                    actionName: request.ProfitCentreCode ?? string.Empty,
                    details: $"Profit Centre '{request.ProfitCentreCode}' added mid-year. Justification: {request.MidYearJustification}. " +
                             "Note: prior transactions cannot be retro-tagged to this profit centre.",
                    module: "ProfitCentre"
                );
                await _mediator.Publish(midYearNote, cancellationToken);
            }

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Profit Centre created successfully.",
                Data = newId
            };
        }
    }
}
