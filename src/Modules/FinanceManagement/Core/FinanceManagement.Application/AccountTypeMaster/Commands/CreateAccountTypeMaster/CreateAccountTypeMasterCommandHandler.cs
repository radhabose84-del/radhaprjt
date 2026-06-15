using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.CreateAccountTypeMaster
{
    public class CreateAccountTypeMasterCommandHandler : IRequestHandler<CreateAccountTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAccountTypeMasterCommandHandler(
            IAccountTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateAccountTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AccountTypeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ACCOUNT_TYPE_MASTER_CREATE",
                actionName: request.AccountTypeName ?? string.Empty,
                details: $"Account Type Master '{request.AccountTypeName}' created successfully with Id {newId}.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Type Master created successfully.",
                Data = newId
            };
        }
    }
}
