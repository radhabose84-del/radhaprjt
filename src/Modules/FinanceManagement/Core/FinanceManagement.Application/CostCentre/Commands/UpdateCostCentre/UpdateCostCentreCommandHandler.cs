using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre
{
    public class UpdateCostCentreCommandHandler : IRequestHandler<UpdateCostCentreCommand, ApiResponseDTO<int>>
    {
        private readonly ICostCentreCommandRepository _commandRepository;
        private readonly ICostCentreQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCostCentreCommandHandler(
            ICostCentreCommandRepository commandRepository,
            ICostCentreQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCostCentreCommand request, CancellationToken cancellationToken)
        {
            // Deactivation guard (AC#3). The open-transaction check is a pre-wired stub
            // (returns false) until the journal engine tags transactions to cost centres.
            if (request.IsActive == 0)
            {
                var hasOpen = await _queryRepository.HasOpenTransactionsAsync(request.Id);
                if (hasOpen)
                    throw new ExceptionRules(
                        "This cost centre has open transactions in the current period. Close or reassign these transactions before deactivating.");
            }

            var entity = _mapper.Map<Domain.Entities.CostCentre>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COST_CENTRE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Cost Centre with Id {request.Id} updated successfully.",
                module: "CostCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Cost Centre updated successfully.",
                Data = updatedId
            };
        }
    }
}
