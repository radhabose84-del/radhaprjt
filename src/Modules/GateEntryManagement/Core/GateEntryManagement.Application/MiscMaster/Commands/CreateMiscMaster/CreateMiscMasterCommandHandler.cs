using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster
{
    public class CreateMiscMasterCommandHandler : IRequestHandler<CreateMiscMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMiscMasterCommandHandler(
            IMiscMasterCommandRepository commandRepository,
            IMiscMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMiscMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MiscMaster>(request);

            // Auto-calculate SortOrder
            var maxSortOrder = await _commandRepository.GetMaxSortOrderAsync(request.MiscTypeId);
            entity.SortOrder = maxSortOrder + 1;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MISC_MASTER_CREATE",
                actionName: request.Code ?? string.Empty,
                details: $"MiscMaster '{request.Code}' created successfully with Id {newId}.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Misc Master created successfully.",
                Data = newId
            };
        }
    }
}
