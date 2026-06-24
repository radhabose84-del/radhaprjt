using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod
{
    public class UpdateAccountingPeriodCommandHandler : IRequestHandler<UpdateAccountingPeriodCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountingPeriodCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAccountingPeriodCommandHandler(
            IAccountingPeriodCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateAccountingPeriodCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.AccountingPeriod>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ACCOUNTING_PERIOD_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Accounting Period with Id {request.Id} updated successfully.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Accounting Period updated successfully.",
                Data = updatedId
            };
        }
    }
}
