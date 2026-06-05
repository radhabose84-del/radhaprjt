using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.QC;
using MediatR;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCRQualityTemplateParameters
{
    public class GetOCRQualityTemplateParametersQueryHandler
        : IRequestHandler<GetOCRQualityTemplateParametersQuery, IReadOnlyList<QualityTemplateParameterLookupDto>>
    {
        private readonly IQualityTemplateLookup _qualityTemplateLookup;
        private readonly IMediator _mediator;

        public GetOCRQualityTemplateParametersQueryHandler(
            IQualityTemplateLookup qualityTemplateLookup,
            IMediator mediator)
        {
            _qualityTemplateLookup = qualityTemplateLookup;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<QualityTemplateParameterLookupDto>> Handle(
            GetOCRQualityTemplateParametersQuery request, CancellationToken cancellationToken)
        {
            var parameters = await _qualityTemplateLookup
                .GetParametersByTemplateIdAsync(request.QualityTemplateId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetOCRQualityTemplateParametersQuery",
                actionName: parameters.Count.ToString(),
                details: $"Quality template {request.QualityTemplateId} parameters were fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return parameters;
        }
    }
}
