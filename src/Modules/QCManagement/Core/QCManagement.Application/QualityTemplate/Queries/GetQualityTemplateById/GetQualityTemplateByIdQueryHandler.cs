using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateById
{
    public class GetQualityTemplateByIdQueryHandler : IRequestHandler<GetQualityTemplateByIdQuery, QualityTemplateDto?>
    {
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQualityTemplateByIdQueryHandler(
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<QualityTemplateDto?> Handle(GetQualityTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQualityTemplateByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"QualityTemplate details {dto.Id} was fetched.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
