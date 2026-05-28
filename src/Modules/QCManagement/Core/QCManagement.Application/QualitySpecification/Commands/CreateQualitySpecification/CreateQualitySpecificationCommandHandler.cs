using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Domain.Entities;
using QCManagement.Domain.Events;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification
{
    public class CreateQualitySpecificationCommandHandler : IRequestHandler<CreateQualitySpecificationCommand, ApiResponseDTO<int>>
    {
        private readonly IQualitySpecificationCommandRepository _commandRepository;
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateQualitySpecificationCommandHandler(
            IQualitySpecificationCommandRepository commandRepository,
            IQualitySpecificationQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateQualitySpecificationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualitySpecification>(request);

            var maxSeq = await _queryRepository.GetMaxSpecificationCodeSequenceAsync();
            entity.SpecificationCode = $"QS-{(maxSeq + 1):D4}";

            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                entity.QualitySpecificationParameters = request.Parameters.Select(p => new QualitySpecificationParameter
                {
                    QualityParameterId = p.QualityParameterId,
                    ValidationTypeId = p.ValidationTypeId,
                    MinValue = p.MinValue,
                    MaxValue = p.MaxValue,
                    ExpectedValue = p.ExpectedValue,
                    AllowedValues = (p.AllowedValues != null && p.AllowedValues.Count > 0)
                        ? string.Join("|", p.AllowedValues)
                        : null,
                    SeverityId = p.SeverityId,
                    FailureActionId = p.FailureActionId,
                    IsSamplingRequired = p.IsSamplingRequired,
                    Remarks = p.Remarks,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "QUALITY_SPECIFICATION_CREATE",
                actionName: entity.SpecificationCode ?? string.Empty,
                details: $"QualitySpecification '{entity.SpecificationCode}' created successfully with Id {newId}.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Specification created successfully.",
                Data = newId
            };
        }
    }
}
