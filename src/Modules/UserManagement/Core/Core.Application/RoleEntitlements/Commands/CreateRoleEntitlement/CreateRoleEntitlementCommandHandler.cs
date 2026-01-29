using Core.Domain.Entities;
using Core.Application.Common.Interfaces;
using FluentValidation;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IRoleEntitlement;
using Core.Domain.Events;
using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement
{
    public class CreateRoleEntitlementCommandHandler : IRequestHandler<CreateRoleEntitlementCommand, bool>
    {
        private readonly IRoleEntitlementCommandRepository _roleEntitlementCommandrepository;
        private readonly IRoleEntitlementQueryRepository _roleEntitlementQueryrepository;

        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ILogger<CreateRoleEntitlementCommandHandler> _logger;



        public CreateRoleEntitlementCommandHandler(IRoleEntitlementCommandRepository roleEntitlementCommandrepository,IRoleEntitlementQueryRepository roleEntitlementQueryrepository, IMapper mapper, IMediator mediator,ILogger<CreateRoleEntitlementCommandHandler> logger)
        {
            _roleEntitlementCommandrepository = roleEntitlementCommandrepository;
            _roleEntitlementQueryrepository = roleEntitlementQueryrepository;
            _mapper = mapper;
            _mediator = mediator;    
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<bool> Handle(CreateRoleEntitlementCommand request, CancellationToken cancellationToken)
        {

                _logger.LogInformation("Starting role entitlement creation process for RoleName: {RoleName}");
                
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
                await _roleEntitlementCommandrepository.AddRoleEntitlementsAsync(roleId,roleModules,roleParents,roleChildren,roleMenuPrivileges, cancellationToken);

                    // Domain Event for Audit Logs
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Create",
                        actionCode: "RoleEntitlement",
                        actionName: "Create",
                        details: $"RoleEntitlement  was created.",
                        module: "RoleEntitlement"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
                    _logger.LogInformation("Role entitlements successfully created for RoleName: {RoleName}");

                    return true;
        }

    }

}