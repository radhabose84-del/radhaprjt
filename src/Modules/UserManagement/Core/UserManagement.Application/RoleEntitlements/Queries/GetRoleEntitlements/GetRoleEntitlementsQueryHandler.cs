using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements
{
    public class GetRoleEntitlementsQueryHandler : IRequestHandler<GetRoleEntitlementsQuery, List<RoleEntitlementDto>>
    {
        private readonly IRoleEntitlementQueryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        
    public GetRoleEntitlementsQueryHandler(IRoleEntitlementQueryRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator; 

    }

    #pragma warning disable CS1998
    public async Task<List<RoleEntitlementDto>> Handle(GetRoleEntitlementsQuery request, CancellationToken cancellationToken)
    #pragma warning restore CS1998
    {
        // Fetch role entitlements from the repository
        // var roleEntitlements = await _repository.GetRoleEntitlementsByRoleNameAsync(request.RoleName, cancellationToken);

        //             //Domain Event
        //         var domainEvent = new AuditLogsDomainEvent(
        //             actionDetail: "Create",
        //             actionCode: request.RoleName,
        //             actionName: request.RoleName,
        //             details: $"RoleEntitlement '{request.RoleName}' was created. RoleName: {request.RoleName}",
        //             module:"RoleEntitlement"
        //         );
        //         await _mediator.Publish(domainEvent, cancellationToken);

        // // Map the result to RoleEntitlementDto
        // return _mapper.Map<List<RoleEntitlementDto>>(roleEntitlements);

        return new List<RoleEntitlementDto>();
    }

    }
}