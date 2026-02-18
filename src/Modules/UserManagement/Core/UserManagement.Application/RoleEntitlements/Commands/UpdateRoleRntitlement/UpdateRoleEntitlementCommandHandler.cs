#nullable disable
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using MediatR;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Domain.Events;
using Contracts.Common;
using Microsoft.Extensions.Logging;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement
{
    public class UpdateRoleEntitlementCommandHandler : IRequestHandler<UpdateRoleEntitlementCommand, bool>
    {
     private readonly IRoleEntitlementCommandRepository _roleEntitlementCommanderepository;
     private readonly IRoleEntitlementQueryRepository _roleEntitlementQueryrepository;
     private readonly IMapper _mapper;
     private readonly IMediator _mediator; 
    private readonly ILogger<UpdateRoleEntitlementCommandHandler> _logger;


    public UpdateRoleEntitlementCommandHandler(IRoleEntitlementCommandRepository roleEntitlementCommanderepository, IRoleEntitlementQueryRepository roleEntitlementQueryrepository,IMapper mapper, IMediator mediator,ILogger<UpdateRoleEntitlementCommandHandler> logger)
    {
        _roleEntitlementCommanderepository = roleEntitlementCommanderepository;
        _roleEntitlementQueryrepository = roleEntitlementQueryrepository;
        _mapper = mapper;
        _mediator = mediator;    
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    public async Task<bool> Handle(UpdateRoleEntitlementCommand request, CancellationToken cancellationToken)
    {
        
        var roleId = request.RoleId;

                     var roleModules = request.RoleModules.Select(dto => {
                         var entity = _mapper.Map<RoleModule>(dto);
                         entity.RoleId = roleId;  
                         return entity;
                     }).ToList();

                     var roleParents = request.RoleParents.Select(dto => {
                         var entity = _mapper.Map<RoleParent>(dto);
                         entity.RoleId = roleId; 
                         return entity;
                     }).ToList();

                     var roleChildren = request.RoleChildren.Select(dto => {
                         var entity = _mapper.Map<RoleChild>(dto);
                         entity.RoleId = roleId; 
                         return entity;
                     }).ToList();

                     var roleMenuPrivileges = request.RoleMenuPrivileges.Select(dto => {
                         var entity = _mapper.Map<RoleMenuPrivileges>(dto);
                         entity.RoleId = roleId; 
                         return entity;
                     }).ToList();
            
            var  role = await _roleEntitlementCommanderepository.UpdateRoleEntitlementsAsync(request.RoleId,roleModules,roleParents,roleChildren,roleMenuPrivileges, cancellationToken);

        if (!role)
        {
            throw new Exception("Role entitlements update failed.");
           
        }
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "RoleEntitlement",
            actionName: "Update",
            details: $"RoleEntitlements for Role '{request.RoleId}' were updated.",
            module: "RoleEntitlement"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        return true;
  
    }
    }
}