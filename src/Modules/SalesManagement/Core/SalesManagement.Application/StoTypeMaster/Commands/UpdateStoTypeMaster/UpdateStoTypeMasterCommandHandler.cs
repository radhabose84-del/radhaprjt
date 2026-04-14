using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster
{
    public class UpdateStoTypeMasterCommandHandler : IRequestHandler<UpdateStoTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IStoTypeMasterCommandRepository _commandRepository;
        private readonly IStoTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateStoTypeMasterCommandHandler(
            IStoTypeMasterCommandRepository commandRepository,
            IStoTypeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateStoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsStoTypeMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.StoTypeMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "STO_TYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"STO Type Master with Id {request.Id} updated successfully.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "STO Type Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
