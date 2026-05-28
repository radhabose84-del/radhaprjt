using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Domain.Entities;
using QCManagement.Domain.Events;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification
{
    public class UpdateQualitySpecificationCommandHandler : IRequestHandler<UpdateQualitySpecificationCommand, ApiResponseDTO<int>>
    {
        private readonly IQualitySpecificationCommandRepository _commandRepository;
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateQualitySpecificationCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateQualitySpecificationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualitySpecification>(request);

            if (request.Parameters != null && request.Parameters.Count > 0)
            {
                entity.QualitySpecificationParameters = request.Parameters.Select(p => new QualitySpecificationParameter
                {
                    Id = p.Id,
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
                    IsActive = p.IsActive == 1 ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "QUALITY_SPECIFICATION_UPDATE",
                actionName: request.Id.ToString(),
                details: $"QualitySpecification with Id {request.Id} updated successfully.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Specification updated successfully.",
                Data = result
            };
        }
    }
}
