using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.CreateCostCentre
{
    public class CreateCostCentreCommandHandler : IRequestHandler<CreateCostCentreCommand, ApiResponseDTO<int>>
    {
        private readonly ICostCentreCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCostCentreCommandHandler(
            ICostCentreCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCostCentreCommand request, CancellationToken cancellationToken)
        {
            // Unit-wise scope — both resolved from the JWT, never the payload.
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("No active unit in session.");
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.CostCentre>(request);
            entity.UnitId = unitId;
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COST_CENTRE_CREATE",
                actionName: request.CostCentreCode ?? string.Empty,
                details: $"Cost Centre '{request.CostCentreCode}' ({request.CostCentreName}) created successfully with Id {newId} for Unit {unitId}.",
                module: "CostCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Cost Centre created successfully.",
                Data = newId
            };
        }
    }
}
