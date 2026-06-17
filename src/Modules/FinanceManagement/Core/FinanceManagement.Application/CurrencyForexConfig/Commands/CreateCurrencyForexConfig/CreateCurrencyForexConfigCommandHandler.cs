using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig
{
    public class CreateCurrencyForexConfigCommandHandler : IRequestHandler<CreateCurrencyForexConfigCommand, ApiResponseDTO<int>>
    {
        private readonly ICurrencyForexConfigCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateCurrencyForexConfigCommandHandler(
            ICurrencyForexConfigCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCurrencyForexConfigCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.CurrencyForexConfig>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "CURRENCY_FOREX_CONFIG_CREATE",
                actionName: request.CurrencyTypeCode ?? string.Empty,
                details: $"Currency Forex Config '{request.CurrencyTypeCode}' ({request.CurrencyTypeName}) created successfully with Id {newId} for Company {companyId}.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Currency Forex Config created successfully.",
                Data = newId
            };
        }
    }
}
