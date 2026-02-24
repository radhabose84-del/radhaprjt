#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandHandler : IRequestHandler<UpdateMiscTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscTypeMasterCommandRepository _commandRepository;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMiscTypeMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new EntityNotFoundException($"Misc Type Master with Id {request.Id} not found.");

            var entity = _mapper.Map<Domain.Entities.MiscTypeMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MISC_TYPE_UPDATE",
                actionName: existing.MiscTypeCode,
                details: $"Misc Type Master '{existing.MiscTypeCode}' updated successfully.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Misc Type Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
