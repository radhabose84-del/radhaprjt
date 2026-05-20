using UserManagement.Domain.Entities;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement
{
    public class CreateRoleEntitlementCommandHandler : IRequestHandler<CreateRoleEntitlementCommand, bool>
    {
        private readonly IRoleEntitlementCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateRoleEntitlementCommandHandler> _logger;

        public CreateRoleEntitlementCommandHandler(
            IRoleEntitlementCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateRoleEntitlementCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(CreateRoleEntitlementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving role entitlements for RoleId: {RoleId}", request.RoleId);

            var roleId = request.RoleId;

            var roleModules = request.RoleModules.Select(dto => { var e = _mapper.Map<RoleModule>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleParents = request.RoleParents.Select(dto => { var e = _mapper.Map<RoleParent>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleChildren = request.RoleChildren.Select(dto => { var e = _mapper.Map<RoleChild>(dto); e.RoleId = roleId; return e; }).ToList();
            var roleMenuPrivileges = request.RoleMenuPrivileges.Select(dto => { var e = _mapper.Map<RoleMenuPrivileges>(dto); e.RoleId = roleId; return e; }).ToList();

            await _commandRepository.SaveRoleEntitlementsAsync(roleId, roleModules, roleParents, roleChildren, roleMenuPrivileges, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "RoleEntitlement",
                actionName: "Create",
                details: $"RoleEntitlements for Role '{roleId}' were created.",
                module: "RoleEntitlement"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation("Role entitlements saved for RoleId: {RoleId}", roleId);
            return true;
        }
    }
}
