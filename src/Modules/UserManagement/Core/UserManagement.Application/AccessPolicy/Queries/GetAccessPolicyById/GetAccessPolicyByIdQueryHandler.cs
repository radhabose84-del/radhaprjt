using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyById
{
    public class GetAccessPolicyByIdQueryHandler
        : IRequestHandler<GetAccessPolicyByIdQuery, AccessPolicyDto?>
    {
        private readonly IAccessPolicyQueryRepository _queryRepository;
        private readonly IMapper                      _mapper;
        private readonly IMediator                    _mediator;

        public GetAccessPolicyByIdQueryHandler(
            IAccessPolicyQueryRepository queryRepository,
            IMapper                      mapper,
            IMediator                    mediator)
        {
            _queryRepository = queryRepository;
            _mapper          = mapper;
            _mediator        = mediator;
        }

        public async Task<AccessPolicyDto?> Handle(
            GetAccessPolicyByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode:   "GetAccessPolicyByIdQuery",
                actionName:   result.Id.ToString(),
                details:      $"AccessPolicy details {result.Id} was fetched.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
