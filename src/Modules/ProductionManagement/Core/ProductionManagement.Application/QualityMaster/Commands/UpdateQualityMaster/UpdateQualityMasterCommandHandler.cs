using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster
{
    public class UpdateQualityMasterCommandHandler : IRequestHandler<UpdateQualityMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IQualityMasterCommandRepository _commandRepository;
        private readonly IQualityMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateQualityMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateQualityMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.QualityMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "QUALITYMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Quality Master with Id {request.Id} updated successfully.",
                module: "QualityMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quality Master updated successfully.",
                Data = result
            };
        }
    }
}
