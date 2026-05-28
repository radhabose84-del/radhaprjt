using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateAutoComplete
{
    public class GetQualityTemplateAutoCompleteQueryHandler : IRequestHandler<GetQualityTemplateAutoCompleteQuery, IReadOnlyList<QualityTemplateLookupDto>>
    {
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQualityTemplateAutoCompleteQueryHandler(
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<QualityTemplateLookupDto>> Handle(GetQualityTemplateAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetQualityTemplateAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "QualityTemplate details was fetched.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
