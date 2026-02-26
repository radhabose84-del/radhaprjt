using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingById
{
    public class GetDispatchAddressMappingByIdQueryHandler : IRequestHandler<GetDispatchAddressMappingByIdQuery, DispatchAddressMappingDto>
    {
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAddressMappingByIdQueryHandler(
            IDispatchAddressMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DispatchAddressMappingDto> Handle(GetDispatchAddressMappingByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<DispatchAddressMappingDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDispatchAddressMappingByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"DispatchAddressMapping details {dto.Id} was fetched.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
