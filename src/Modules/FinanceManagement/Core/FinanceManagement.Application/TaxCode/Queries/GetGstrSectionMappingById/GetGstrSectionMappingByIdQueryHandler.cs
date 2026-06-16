using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMappingById
{
    public class GetGstrSectionMappingByIdQueryHandler : IRequestHandler<GetGstrSectionMappingByIdQuery, GstrSectionMappingDto?>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGstrSectionMappingByIdQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GstrSectionMappingDto?> Handle(GetGstrSectionMappingByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetGstrMappingByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<GstrSectionMappingDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetGstrSectionMappingByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"GstrSectionMapping details {dto.Id} was fetched.",
                module: "GstrSectionMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
