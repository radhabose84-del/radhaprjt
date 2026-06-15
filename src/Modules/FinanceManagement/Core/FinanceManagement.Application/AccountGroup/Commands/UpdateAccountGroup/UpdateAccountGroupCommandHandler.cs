using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup
{
    public class UpdateAccountGroupCommandHandler : IRequestHandler<UpdateAccountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAccountGroupCommandHandler(
            IAccountGroupCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateAccountGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AccountGroup>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ACCOUNT_GROUP_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Account Group with Id {request.Id} updated successfully.",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Group updated successfully.",
                Data = result
            };
        }
    }
}
