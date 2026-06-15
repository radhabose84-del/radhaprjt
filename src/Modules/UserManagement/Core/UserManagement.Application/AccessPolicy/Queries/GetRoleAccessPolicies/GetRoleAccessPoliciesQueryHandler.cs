using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies
{
    public class GetRoleAccessPoliciesQueryHandler
        : IRequestHandler<GetRoleAccessPoliciesQuery, List<RoleAccessPolicyDto>>
    {
        private readonly IAccessPolicyQueryRepository _queryRepository;
        private readonly IMapper                      _mapper;
        private readonly IMediator                    _mediator;

        public GetRoleAccessPoliciesQueryHandler(
            IAccessPolicyQueryRepository queryRepository,
            IMapper                      mapper,
            IMediator                    mediator)
        {
            _queryRepository = queryRepository;
            _mapper          = mapper;
            _mediator        = mediator;
        }

        public async Task<List<RoleAccessPolicyDto>> Handle(
            GetRoleAccessPoliciesQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetRoleAccessPoliciesAsync(
                request.AccessPolicyId, request.RoleId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode:   "GetRoleAccessPoliciesQuery",
                actionName:   result.Count.ToString(),
                details:      $"RoleAccessPolicy details for AccessPolicyId {request.AccessPolicyId} were fetched.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
