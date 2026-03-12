using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.CreateProcurementType
{
    public class CreateProcurementTypeCommandHandler : IRequestHandler<CreateProcurementTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IProcurementTypeCommandRepository _commandRepository;
        private readonly IProcurementTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateProcurementTypeCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateProcurementTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProcurementType>(request);

            // Auto-generate ProcurementCode: first 3 uppercase chars of ProcurementName + 4-digit sequential
            var prefix = request.ProcurementName!.Length >= 3
                ? request.ProcurementName.Substring(0, 3).ToUpper()
                : request.ProcurementName.ToUpper().PadRight(3, 'X');

            var generatedCode = await _queryRepository.GetNextProcurementCodeAsync(prefix);
            entity.ProcurementCode = generatedCode;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PROCUREMENTTYPE_CREATE",
                actionName: generatedCode,
                details: $"ProcurementType '{generatedCode}' created successfully with Id {newId}.",
                module: "ProcurementType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "ProcurementType created successfully.",
                Data = newId
            };
        }
    }
}
