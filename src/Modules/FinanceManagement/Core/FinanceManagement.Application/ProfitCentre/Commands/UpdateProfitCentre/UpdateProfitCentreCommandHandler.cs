using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre
{
    public class UpdateProfitCentreCommandHandler : IRequestHandler<UpdateProfitCentreCommand, ApiResponseDTO<int>>
    {
        private readonly IProfitCentreCommandRepository _commandRepository;
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateProfitCentreCommandHandler(
            IProfitCentreCommandRepository commandRepository,
            IProfitCentreQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateProfitCentreCommand request, CancellationToken cancellationToken)
        {
            // Deactivation guard (AC#5). The current-year-transaction check is a pre-wired stub
            // (returns false) until the journal engine tags transactions to profit centres.
            if (request.IsActive == 0)
            {
                var hasCurrentYear = await _queryRepository.HasCurrentYearTransactionsAsync(request.Id);
                if (hasCurrentYear)
                    throw new ExceptionRules(
                        "This profit centre has transactions in the current financial year. Deactivation is blocked until year-end close.");
            }

            var entity = _mapper.Map<Domain.Entities.ProfitCentre>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PROFIT_CENTRE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Profit Centre with Id {request.Id} updated successfully.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Profit Centre updated successfully.",
                Data = updatedId
            };
        }
    }
}
