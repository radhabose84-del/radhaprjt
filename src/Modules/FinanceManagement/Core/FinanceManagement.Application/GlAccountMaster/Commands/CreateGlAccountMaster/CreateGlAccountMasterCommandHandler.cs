using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster
{
    public class CreateGlAccountMasterCommandHandler : IRequestHandler<CreateGlAccountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IGlAccountMasterCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateGlAccountMasterCommandHandler(
            IGlAccountMasterCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGlAccountMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.GlAccountMaster>(request);
            entity.CompanyId = companyId;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GL_ACCOUNT_MASTER_CREATE",
                actionName: request.AccountCode ?? string.Empty,
                details: $"GL Account '{request.AccountCode}' ({request.AccountName}) created successfully with Id {newId} for Company {companyId}.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GL Account created successfully.",
                Data = newId
            };
        }
    }
}
