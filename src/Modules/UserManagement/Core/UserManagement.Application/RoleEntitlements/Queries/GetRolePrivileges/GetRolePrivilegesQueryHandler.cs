using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges
{
    public class GetRolePrivilegesQueryHandler : IRequestHandler<GetRolePrivilegesQuery, List<ModuleDTO>>
    {
        private readonly IRoleEntitlementQueryRepository _roleEntitlementQueryRepository;
        private readonly IMapper _mapper;
        public GetRolePrivilegesQueryHandler(IRoleEntitlementQueryRepository roleEntitlementQueryRepository,IMapper mapper)
        {
            _roleEntitlementQueryRepository = roleEntitlementQueryRepository;
            _mapper = mapper;
        }
        public async Task<List<ModuleDTO>> Handle(GetRolePrivilegesQuery request, CancellationToken cancellationToken)
        {
            var rolePrivileges =await _roleEntitlementQueryRepository.GetRolePrivileges(request.UserId,cancellationToken);
            var rolePrivilegesMap= _mapper.Map<List<ModuleDTO>>(rolePrivileges);

            return rolePrivilegesMap;
        }
    }
}