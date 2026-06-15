using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy
{
    public class CreateAccessPolicyCommandHandler : IRequestHandler<CreateAccessPolicyCommand, ApiResponseDTO<int>>
    {
        private readonly IAccessPolicyCommandRepository _commandRepository;
        private readonly IAccessPolicyQueryRepository   _queryRepository;
        private readonly IMediator                      _mediator;
        private readonly IMapper                        _mapper;

        public CreateAccessPolicyCommandHandler(
            IAccessPolicyCommandRepository commandRepository,
            IAccessPolicyQueryRepository   queryRepository,
            IMediator                      mediator,
            IMapper                        mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository   = queryRepository;
            _mediator          = mediator;
            _mapper            = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AccessPolicy>(request);
            var newId  = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode:   "ACCESSPOLICY_CREATE",
                actionName:   request.PolicyCode,
                details:      $"AccessPolicy '{request.PolicyCode}' created successfully with Id {newId}.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message   = "Access Policy created successfully.",
                Data      = newId
            };
        }
    }
}
