using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup
{
    public class CreateAccountGroupCommandHandler : IRequestHandler<CreateAccountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountGroupCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAccountGroupCommandHandler(
            IAccountGroupCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateAccountGroupCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.AccountGroup>(request);
            entity.CompanyId = companyId;

            // Level is derived from the parent inside the repository.
            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ACCOUNT_GROUP_CREATE",
                actionName: request.GroupCode,
                details: $"Account Group '{request.GroupCode}' created successfully with Id {newId}.",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Group created successfully.",
                Data = newId
            };
        }
    }
}
