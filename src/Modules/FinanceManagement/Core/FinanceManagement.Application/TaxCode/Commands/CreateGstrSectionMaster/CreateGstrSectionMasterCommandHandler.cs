using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster
{
    public class CreateGstrSectionMasterCommandHandler : IRequestHandler<CreateGstrSectionMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IGstrSectionCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateGstrSectionMasterCommandHandler(
            IGstrSectionCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGstrSectionMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.GstrSectionMaster>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateSectionAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GSTR_SECTION_MASTER_CREATE",
                actionName: request.SectionCode ?? string.Empty,
                details: $"GSTR section '{request.SectionCode}' created with Id {newId}.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GSTR section created successfully.",
                Data = newId
            };
        }
    }
}
