using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod
{
    public class CreateAccountingPeriodCommandHandler : IRequestHandler<CreateAccountingPeriodCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountingPeriodCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAccountingPeriodCommandHandler(
            IAccountingPeriodCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateAccountingPeriodCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<FinanceManagement.Domain.Entities.AccountingPeriod>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ACCOUNTING_PERIOD_CREATE",
                actionName: request.PeriodName ?? string.Empty,
                details: $"Accounting Period '{request.PeriodName}' created successfully with Id {newId}.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Accounting Period created successfully.",
                Data = newId
            };
        }
    }
}
