using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType
{
    public class CreateRawMaterialTypeCommandHandler : IRequestHandler<CreateRawMaterialTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IRawMaterialTypeCommandRepository _commandRepository;
        private readonly IRawMaterialTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateRawMaterialTypeCommandHandler(
            IRawMaterialTypeCommandRepository commandRepository,
            IRawMaterialTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateRawMaterialTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RawMaterialType>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "RAWMATERIALTYPE_CREATE",
                actionName: request.RawMaterialTypeCode,
                details: $"Raw Material Type '{request.RawMaterialTypeCode}' created successfully with Id {newId}.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Raw Material Type created successfully.",
                Data = newId
            };
        }
    }
}
