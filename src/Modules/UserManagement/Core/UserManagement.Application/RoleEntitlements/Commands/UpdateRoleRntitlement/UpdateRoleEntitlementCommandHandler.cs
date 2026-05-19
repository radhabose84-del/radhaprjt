using UserManagement.Domain.Entities;
using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement
{
    public class UpdateRoleEntitlementCommandHandler : IRequestHandler<UpdateRoleEntitlementCommand, bool>
    {
        private readonly IRoleEntitlementCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateRoleEntitlementCommandHandler> _logger;

        public UpdateRoleEntitlementCommandHandler(
            IRoleEntitlementCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator,
            ILogger<UpdateRoleEntitlementCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateRoleEntitlementCommand request, CancellationToken cancellationToken)
        {
            var roleId = request.RoleId;

            var roleModules = request.RoleModules.Select(dto => { var e = _mapper.Map<RoleModule>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleParents = request.RoleParents.Select(dto => { var e = _mapper.Map<RoleParent>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleChildren = request.RoleChildren.Select(dto => { var e = _mapper.Map<RoleChild>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleMenuPrivileges = request.RoleMenuPrivileges.Select(dto => { var e = _mapper.Map<RoleMenuPrivileges>(dto); e.RoleId = roleId; return e; }).ToList();

            var saved = await _commandRepository.SaveRoleEntitlementsAsync(roleId, roleModules, roleParents, roleChildren, roleMenuPrivileges, cancellationToken);

            if (!saved)
                throw new Exception("Role entitlements update failed.");

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "RoleEntitlement",
                actionName: "Update",
                details: $"RoleEntitlements for Role '{roleId}' were updated.",
                module: "RoleEntitlement"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }
    }
}
