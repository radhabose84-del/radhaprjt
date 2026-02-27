using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster
{
    public class UpdateDispatchAddressMasterCommandHandler : IRequestHandler<UpdateDispatchAddressMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAddressMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDispatchAddressMasterCommandHandler(
            IDispatchAddressMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateDispatchAddressMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.DispatchAddressMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DISPATCH_ADDRESS_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Address Master with Id {request.Id} updated successfully.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Address Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
