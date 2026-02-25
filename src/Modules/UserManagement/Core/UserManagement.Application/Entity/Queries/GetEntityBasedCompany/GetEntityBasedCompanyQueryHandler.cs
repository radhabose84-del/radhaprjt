using AutoMapper;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntityBasedCompany
{
    public class GetEntityBasedCompanyQueryHandler : IRequestHandler<GetEntityBasedCompanyQuery, List<GetEntityBasedCompanyDto>>
    {
        private readonly IEntityQueryRepository _ientityQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetEntityBasedCompanyQueryHandler(IEntityQueryRepository IentityQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ientityQueryRepository = IentityQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetEntityBasedCompanyDto>> Handle(GetEntityBasedCompanyQuery request, CancellationToken cancellationToken)
        {
            // Fetch data from repository
            var result = await _ientityQueryRepository.GetCompanyNames(request.EntityId);

            // Map to DTO (if needed; here it's already GetEntityBasedCompanyDto, but mapping can be used for consistency)
            var companies = _mapper.Map<List<GetEntityBasedCompanyDto>>(result);

            // Domain Event for auditing
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetEntityBasedCompanyQuery",
                actionName: companies.Count.ToString(),
                details: $"Company details were fetched for EntityId: {request.EntityId}.",
                module: "Entity"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return companies;
        }
    }
}