using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById
{
    public class GetDocumentSequenceByIdQueryHandler : IRequestHandler<GetDocumentSequenceByIdQuery, DocumentSequenceDto?>
    {
        private readonly IDocumentSequenceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDocumentSequenceByIdQueryHandler(
            IDocumentSequenceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DocumentSequenceDto?> Handle(GetDocumentSequenceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<DocumentSequenceDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDocumentSequenceByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Document Sequence details {dto.Id} was fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
