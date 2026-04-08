using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.LotMaster.Commands.UpdateLotMaster
{
    public class UpdateLotMasterCommandHandler : IRequestHandler<UpdateLotMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ILotMasterCommandRepository _commandRepository;
        private readonly ILotMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateLotMasterCommandHandler(
            ILotMasterCommandRepository commandRepository,
            ILotMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateLotMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsLotMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new Contracts.Common.ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.LotMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "LOTMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"LotMaster with Id {request.Id} updated successfully.",
                module: "LotMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Lot Master updated successfully.",
                Data = result
            };
        }
    }
}
