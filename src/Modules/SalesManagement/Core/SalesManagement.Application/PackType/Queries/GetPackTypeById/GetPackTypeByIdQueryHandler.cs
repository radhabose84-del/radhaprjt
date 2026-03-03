using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Application.PackType.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.PackType.Queries.GetPackTypeById
{
    public class GetPackTypeByIdQueryHandler : IRequestHandler<GetPackTypeByIdQuery, PackTypeDto?>
    {
        private readonly IPackTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPackTypeByIdQueryHandler(IPackTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<PackTypeDto?> Handle(GetPackTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var packType = _mapper.Map<PackTypeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPackTypeByIdQuery",
                actionName: packType.Id.ToString(),
                details: $"PackType details {packType.Id} was fetched.",
                module: "PackType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return packType;
        }
    }
}
