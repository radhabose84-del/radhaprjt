using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster
{
    public class CreateTaxCodeMasterCommandHandler : IRequestHandler<CreateTaxCodeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTaxCodeMasterCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTaxCodeMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.TaxCodeMaster>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateTaxCodeAsync(entity);

            // Initial effective-dated rate (VersionNo set by the repository).
            await _commandRepository.CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = newId,
                RatePercent = request.RatePercent,
                EffectiveFrom = request.EffectiveFrom
            });

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TAX_CODE_MASTER_CREATE",
                actionName: request.TaxCode ?? string.Empty,
                details: $"Tax Code '{request.TaxCode}' created successfully with Id {newId}.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax Code created successfully.",
                Data = newId
            };
        }
    }
}
