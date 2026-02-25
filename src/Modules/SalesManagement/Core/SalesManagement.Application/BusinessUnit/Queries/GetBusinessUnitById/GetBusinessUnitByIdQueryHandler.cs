#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById
{
    public class GetBusinessUnitByIdQueryHandler : IRequestHandler<GetBusinessUnitByIdQuery, BusinessUnitDto>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetBusinessUnitByIdQueryHandler(IBusinessUnitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<BusinessUnitDto> Handle(GetBusinessUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result is null)
                return null;

            var businessUnit = _mapper.Map<BusinessUnitDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetBusinessUnitByIdQuery",
                actionName: businessUnit.Id.ToString(),
                details: $"BusinessUnit details {businessUnit.Id} was fetched.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return businessUnit;
        }
    }
}
