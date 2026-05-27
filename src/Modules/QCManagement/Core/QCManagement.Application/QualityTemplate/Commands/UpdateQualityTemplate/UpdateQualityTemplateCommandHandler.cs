using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Domain.Entities;
using QCManagement.Domain.Events;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate
{
    public class UpdateQualityTemplateCommandHandler : IRequestHandler<UpdateQualityTemplateCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityTemplateCommandRepository _commandRepository;
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateQualityTemplateCommandHandler(
            IQualityTemplateCommandRepository commandRepository,
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateQualityTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityTemplate>(request);

            // Map child collection — repository will replace old children with these
            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                entity.QualityTemplateParameters = request.Parameters.Select(p => new QualityTemplateParameter
                {
                    QualityParameterId = p.QualityParameterId,
                    SequenceNo = p.SequenceNo,
                    IsMandatory = p.IsMandatory,
                    IsCritical = p.IsCritical,
                    InspectionMethodId = p.InspectionMethodId,
                    SampleSize = p.SampleSize,
                    SampleUomId = p.SampleUomId,
                    IsGradeApplicable = p.IsGradeApplicable,
                    Remarks = p.Remarks,
                    IsActive = p.IsActive == 1 ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "QUALITY_TEMPLATE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"QualityTemplate with Id {request.Id} updated successfully.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Template updated successfully.",
                Data = result
            };
        }
    }
}
