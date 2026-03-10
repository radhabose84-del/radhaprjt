using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete
{
    public class GetDocumentSequenceAutoCompleteQueryHandler : IRequestHandler<GetDocumentSequenceAutoCompleteQuery, IReadOnlyList<DocumentSequenceLookupDto>>
    {
        private readonly IDocumentSequenceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDocumentSequenceAutoCompleteQueryHandler(
            IDocumentSequenceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DocumentSequenceLookupDto>> Handle(GetDocumentSequenceAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<DocumentSequenceLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDocumentSequenceAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Document Sequence details was fetched.",
                module: "DocumentSequence"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
