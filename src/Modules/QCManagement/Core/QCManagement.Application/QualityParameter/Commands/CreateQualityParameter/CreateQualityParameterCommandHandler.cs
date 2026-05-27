using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Commands.CreateQualityParameter
{
    public class CreateQualityParameterCommandHandler : IRequestHandler<CreateQualityParameterCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityParameterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateQualityParameterCommandHandler(
            IQualityParameterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateQualityParameterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityParameter>(request);

            var nextSeq = await _commandRepository.GetMaxParameterCodeSequenceAsync() + 1;
            entity.ParameterCode = $"QP-{nextSeq:D6}";

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "QUALITY_PARAMETER_CREATE",
                actionName: entity.ParameterCode,
                details: $"Quality Parameter '{entity.ParameterCode}' created successfully with Id {newId}.",
                module: "QualityParameter"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Parameter created successfully.",
                Data = newId
            };
        }
    }
}
