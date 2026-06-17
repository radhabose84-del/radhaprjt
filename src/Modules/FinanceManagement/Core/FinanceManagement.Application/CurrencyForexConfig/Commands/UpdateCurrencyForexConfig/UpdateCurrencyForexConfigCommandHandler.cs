using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig
{
    public class UpdateCurrencyForexConfigCommandHandler : IRequestHandler<UpdateCurrencyForexConfigCommand, ApiResponseDTO<int>>
    {
        private readonly ICurrencyForexConfigCommandRepository _commandRepository;
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCurrencyForexConfigCommandHandler(
            ICurrencyForexConfigCommandRepository commandRepository,
            ICurrencyForexConfigQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCurrencyForexConfigCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsCurrencyForexConfigLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This Currency Forex Config is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.CurrencyForexConfig>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "CURRENCY_FOREX_CONFIG_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Currency Forex Config with Id {request.Id} updated successfully.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Currency Forex Config updated successfully.",
                Data = updatedId
            };
        }
    }
}
