using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderAutoComplete
{
    public class GetYarnConversionHeaderAutoCompleteQueryHandler
        : IRequestHandler<GetYarnConversionHeaderAutoCompleteQuery, IReadOnlyList<YarnConversionHeaderLookupDto>>
    {
        private readonly IYarnConversionHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnConversionHeaderAutoCompleteQueryHandler(
            IYarnConversionHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<YarnConversionHeaderLookupDto>> Handle(
            GetYarnConversionHeaderAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetYarnConversionHeaderAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "YarnConversionHeader autocomplete was fetched.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
