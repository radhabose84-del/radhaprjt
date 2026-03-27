using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster
{
    public class CreateQualityMasterCommandHandler : IRequestHandler<CreateQualityMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityMasterCommandRepository _commandRepository;
        private readonly IQualityMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateQualityMasterCommandHandler(
            IQualityMasterCommandRepository commandRepository,
            IQualityMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateQualityMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "QUALITYMASTER_CREATE",
                actionName: request.QualityName ?? string.Empty,
                details: $"Quality Master '{request.QualityName}' created successfully with Id {newId}.",
                module: "QualityMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Master created successfully.",
                Data = newId
            };
        }
    }
}
