using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter
{
    public class UpdateQualityParameterCommandHandler : IRequestHandler<UpdateQualityParameterCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityParameterCommandRepository _commandRepository;
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateQualityParameterCommandHandler(
            IQualityParameterCommandRepository commandRepository,
            IQualityParameterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateQualityParameterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityParameter>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "QUALITY_PARAMETER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Quality Parameter with Id {request.Id} updated successfully.",
                module: "QualityParameter"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Parameter updated successfully.",
                Data = updatedId
            };
        }
    }
}
