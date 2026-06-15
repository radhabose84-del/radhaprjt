using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyAutoComplete
{
    public class GetAccessPolicyAutoCompleteQueryHandler
        : IRequestHandler<GetAccessPolicyAutoCompleteQuery, IReadOnlyList<AccessPolicyDto>>
    {
        private readonly IAccessPolicyQueryRepository _queryRepository;
        private readonly IMapper                      _mapper;
        private readonly IMediator                    _mediator;

        public GetAccessPolicyAutoCompleteQueryHandler(
            IAccessPolicyQueryRepository queryRepository,
            IMapper                      mapper,
            IMediator                    mediator)
        {
            _queryRepository = queryRepository;
            _mapper          = mapper;
            _mediator        = mediator;
        }

        public async Task<IReadOnlyList<AccessPolicyDto>> Handle(
            GetAccessPolicyAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode:   "GetAccessPolicyAutoCompleteQuery",
                actionName:   result.Count.ToString(),
                details:      "AccessPolicy details was fetched.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
