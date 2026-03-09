using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTypeId
{
    public class GetDocumentNumberByTypeIdQueryHandler : IRequestHandler<GetDocumentNumberByTypeIdQuery, IReadOnlyList<DocumentSequenceGeneratedDto>>
    {
        private readonly IDocumentSequenceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDocumentNumberByTypeIdQueryHandler(
            IDocumentSequenceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DocumentSequenceGeneratedDto>> Handle(GetDocumentNumberByTypeIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByTypeIdAsync(request.TypeId);
            var dtos = _mapper.Map<List<DocumentSequenceGeneratedDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByTypeId",
                actionCode: "GetDocumentNumberByTypeIdQuery",
                actionName: request.TypeId.ToString(),
                details: $"Document Sequence generated numbers for TypeId {request.TypeId} were fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
