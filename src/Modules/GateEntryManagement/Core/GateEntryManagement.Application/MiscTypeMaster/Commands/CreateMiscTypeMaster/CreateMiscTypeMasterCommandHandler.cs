using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommandHandler : IRequestHandler<CreateMiscTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscTypeMasterCommandRepository _commandRepository;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMiscTypeMasterCommandHandler(
            IMiscTypeMasterCommandRepository commandRepository,
            IMiscTypeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MiscTypeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MISC_TYPE_CREATE",
                actionName: request.MiscTypeCode ?? string.Empty,
                details: $"MiscTypeMaster '{request.MiscTypeCode}' created successfully with Id {newId}.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Misc Type Master created successfully.",
                Data = newId
            };
        }
    }
}
