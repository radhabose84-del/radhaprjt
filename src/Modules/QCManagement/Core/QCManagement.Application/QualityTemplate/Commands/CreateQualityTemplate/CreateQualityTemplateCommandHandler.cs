using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Domain.Entities;
using QCManagement.Domain.Events;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate
{
    public class CreateQualityTemplateCommandHandler : IRequestHandler<CreateQualityTemplateCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityTemplateCommandRepository _commandRepository;
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateQualityTemplateCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateQualityTemplateCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityTemplate>(request);

            // Auto-generate TemplateCode (QT-000001 format)
            var maxSeq = await _queryRepository.GetMaxTemplateCodeSequenceAsync();
            entity.TemplateCode = $"QT-{(maxSeq + 1):D6}";

            // Map child collection
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
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "QUALITY_TEMPLATE_CREATE",
                actionName: entity.TemplateCode ?? string.Empty,
                details: $"QualityTemplate '{entity.TemplateCode}' created successfully with Id {newId}.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Template created successfully.",
                Data = newId
            };
        }
    }
}
