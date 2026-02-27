using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster
{
    public class CreateMiscMasterCommandHandler : IRequestHandler<CreateMiscMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateMiscMasterCommandHandler(
            IMiscMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateMiscMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MiscMaster>(request);
            entity.SortOrder = await _commandRepository.GetMaxSortOrderAsync(request.MiscTypeId) + 1;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MISC_MASTER_CREATE",
                actionName: request.Code ?? string.Empty,
                details: $"Misc Master '{request.Code}' created successfully with Id {newId}.",
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
