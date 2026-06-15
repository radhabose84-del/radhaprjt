using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IMiscMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.MiscMaster.Commands.UpdateMiscMaster
{
    public class UpdateMiscMasterCommandHandler : IRequestHandler<UpdateMiscMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMiscMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateMiscMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsMiscMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

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
