using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster
{
    public class DeleteHSNMasterCommandHandler : IRequestHandler<DeleteHSNMasterCommand, ApiResponseDTO<bool>>
    {
        private readonly IHSNMasterCommandRepository _iHSNMasterCommandRepository;
        private readonly IHSNMasterQueryRepository _hSNMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteHSNMasterCommandHandler(IHSNMasterCommandRepository iHSNMasterCommandRepository, IHSNMasterQueryRepository hSNMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _iHSNMasterCommandRepository = iHSNMasterCommandRepository;
            _hSNMasterQueryRepository = hSNMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<bool>> Handle(DeleteHSNMasterCommand request, CancellationToken cancellationToken)
        {
              if (await _hSNMasterQueryRepository.NotFoundAsync(request.Id))
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"HSN Master with Id {request.Id} not found or already deleted.",
                    Data = false
                };
            }   

            var hSNMaster = _mapper.Map<InventoryManagement.Domain.Entities.HSNMaster>(request);

            var deleted = await _iHSNMasterCommandRepository.DeleteAsync(request.Id, hSNMaster);

            if (!deleted)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"Failed to delete HSN Master with Id {request.Id}.",
                    Data = false
                };
            }           
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "HSN_DELETE",
                actionName: request.Id.ToString(),
                details: $"HSN Master Id {request.Id} was soft deleted.",
                module: "HSNMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "HSN Master soft deleted successfully.",
                Data = true
            };
        }
    }
}