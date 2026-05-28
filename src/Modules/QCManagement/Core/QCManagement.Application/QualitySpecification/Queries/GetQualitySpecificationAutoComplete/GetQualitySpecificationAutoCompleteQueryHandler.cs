using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationAutoComplete
{
    public class GetQualitySpecificationAutoCompleteQueryHandler : IRequestHandler<GetQualitySpecificationAutoCompleteQuery, IReadOnlyList<QualitySpecificationLookupDto>>
    {
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQualitySpecificationAutoCompleteQueryHandler(
            IQualitySpecificationQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<QualitySpecificationLookupDto>> Handle(GetQualitySpecificationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetQualitySpecificationAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "QualitySpecification details was fetched.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
