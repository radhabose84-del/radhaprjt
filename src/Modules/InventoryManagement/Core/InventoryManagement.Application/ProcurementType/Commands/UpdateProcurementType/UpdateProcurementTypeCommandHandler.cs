using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.UpdateProcurementType
{
    public class UpdateProcurementTypeCommandHandler : IRequestHandler<UpdateProcurementTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IProcurementTypeCommandRepository _commandRepository;
        private readonly IProcurementTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateProcurementTypeCommandHandler(
            IProcurementTypeCommandRepository commandRepository,
            IProcurementTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateProcurementTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProcurementType>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PROCUREMENTTYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"ProcurementType with Id {request.Id} updated successfully.",
                module: "ProcurementType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "ProcurementType updated successfully.",
                Data = result
            };
        }
    }
}
