using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscMaster.Commands.UpdateMiscMaster
{
    public class UpdateMiscMasterCommandHandler : IRequestHandler<UpdateMiscMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMiscMasterCommandHandler(
            IMiscMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateMiscMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MiscMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MISC_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Misc Master with Id {request.Id} updated successfully.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Misc Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
