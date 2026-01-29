using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IRoleEntitlement;
using Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using Core.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using Core.Domain.Entities;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.RoleEntitlements.Queries.GetRoleEntitlementById
{
    public class GetEntitlementByIdQueryHandler : IRequestHandler<GetRoleEntitlementByIdQuery, GetByIdRoleEntitlementDTO>
    {
        private readonly IRoleEntitlementQueryRepository _roleEntitlementRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetEntitlementByIdQueryHandler(IRoleEntitlementCommandRepository roleEntitlementCommandRepository, IRoleEntitlementQueryRepository roleEntitlementQueryRepository, IMapper mapper, IMediator mediator)
        {
            _roleEntitlementRepository = roleEntitlementQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GetByIdRoleEntitlementDTO> Handle(GetRoleEntitlementByIdQuery request, CancellationToken cancellationToken)
        {
              var (roleId, roleModules, parentMenus, childMenus, roleMenuPrivileges) = await _roleEntitlementRepository.GetByIdAsync(request.Id);

            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetById",        
                actionName: "GetById",                
                details: $"RoleEntitlement  was created. ",
                module:"RoleEntitlement"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
           var result = _mapper.Map<GetByIdRoleEntitlementDTO>((roleId.Id, roleModules, parentMenus, childMenus, roleMenuPrivileges));

                 return result;          

        }
     

    }
}