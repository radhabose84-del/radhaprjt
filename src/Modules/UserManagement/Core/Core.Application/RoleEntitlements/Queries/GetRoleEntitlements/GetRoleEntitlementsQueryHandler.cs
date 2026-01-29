using AutoMapper;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IRoleEntitlement;
using Core.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.RoleEntitlements.Queries.GetRoleEntitlements
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

    public async Task<List<RoleEntitlementDto>> Handle(GetRoleEntitlementsQuery request, CancellationToken cancellationToken)
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