using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster
{
    public class UpdateHSNMasterCommandHandler : IRequestHandler<UpdateHSNMasterCommand, ApiResponseDTO<int>>
    {

        private readonly IHSNMasterCommandRepository _iHSNMasterCommandRepository;

        private readonly IHSNMasterQueryRepository _hSNMasterQueryRepository;

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateHSNMasterCommandHandler(IHSNMasterCommandRepository iHSNMasterCommandRepository, IHSNMasterQueryRepository hSNMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _iHSNMasterCommandRepository = iHSNMasterCommandRepository;
            _hSNMasterQueryRepository = hSNMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateHSNMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.HSNMaster>(request);

            // ✅ 5. Update
            var updatedId = await _iHSNMasterCommandRepository.UpdateAsync(entity);

            // ✅ 6. Audit Log Event
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "HSN_UPDATE",
                actionName: request.HSNCode,
                details: $"HSN Master '{request.HSNCode}' updated successfully with Id {updatedId}.",
                module: "HSNMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "HSN Master updated successfully.",
                Data = updatedId
            };

        }

    }
}