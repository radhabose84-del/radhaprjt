using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationById
{
    public class GetQualitySpecificationByIdQueryHandler : IRequestHandler<GetQualitySpecificationByIdQuery, QualitySpecificationDto?>
    {
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQualitySpecificationByIdQueryHandler(
            IQualitySpecificationQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<QualitySpecificationDto?> Handle(GetQualitySpecificationByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQualitySpecificationByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"QualitySpecification details {dto.Id} was fetched.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
