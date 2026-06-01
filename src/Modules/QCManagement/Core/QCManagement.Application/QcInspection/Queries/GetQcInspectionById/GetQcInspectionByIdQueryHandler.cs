using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Queries.GetQcInspectionById
{
    public class GetQcInspectionByIdQueryHandler : IRequestHandler<GetQcInspectionByIdQuery, QcInspectionDto?>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetQcInspectionByIdQueryHandler(IQcInspectionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<QcInspectionDto?> Handle(GetQcInspectionByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);
            if (dto == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQcInspectionByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"QC Inspection details {dto.Id} was fetched.",
                module: "QcInspection"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
