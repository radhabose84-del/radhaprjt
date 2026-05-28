using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Queries.GetQualityParameterAutoComplete
{
    public class GetQualityParameterAutoCompleteQueryHandler : IRequestHandler<GetQualityParameterAutoCompleteQuery, IReadOnlyList<QualityParameterLookupDto>>
    {
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetQualityParameterAutoCompleteQueryHandler(IQualityParameterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<QualityParameterLookupDto>> Handle(GetQualityParameterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var dtos = _mapper.Map<List<QualityParameterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetQualityParameterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "QualityParameter details was fetched.",
                module: "QualityParameter"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
