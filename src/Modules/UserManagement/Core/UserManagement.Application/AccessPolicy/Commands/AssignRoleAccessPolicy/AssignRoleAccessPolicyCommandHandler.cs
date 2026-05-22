using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy
{
    public class AssignRoleAccessPolicyCommandHandler : IRequestHandler<AssignRoleAccessPolicyCommand, ApiResponseDTO<int>>
    {
        private readonly IAccessPolicyCommandRepository _commandRepository;
        private readonly IAccessPolicyQueryRepository   _queryRepository;
        private readonly IMediator                      _mediator;
        private readonly IMapper                        _mapper;

        public AssignRoleAccessPolicyCommandHandler(
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
            AssignRoleAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RoleAccessPolicy>(request);
            var newId  = await _commandRepository.AssignRoleValueAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode:   "ROLEACCESSPOLICY_ASSIGN",
                actionName:   $"Policy:{request.AccessPolicyId} Role:{request.RoleId} Value:{request.ValueId}",
                details:      $"RoleAccessPolicy assigned — AccessPolicyId {request.AccessPolicyId}, RoleId {request.RoleId}, ValueId {request.ValueId}. New Id {newId}.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message   = "Role access policy assigned successfully.",
                Data      = newId
            };
        }
    }
}
