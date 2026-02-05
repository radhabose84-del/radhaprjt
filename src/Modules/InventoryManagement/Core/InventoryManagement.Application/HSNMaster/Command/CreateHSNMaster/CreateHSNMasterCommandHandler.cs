using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster
{
    public class CreateHSNMasterCommandHandler : IRequestHandler<CreateHSNMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IHSNMasterCommandRepository _iHSNMasterCommandRepository;
        private readonly IHSNMasterQueryRepository _iHSNMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateHSNMasterCommandHandler(IHSNMasterCommandRepository iHSNMasterCommand, IHSNMasterQueryRepository iHSNMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _iHSNMasterCommandRepository = iHSNMasterCommand;
            _iHSNMasterQueryRepository = iHSNMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }
        
         public async Task<ApiResponseDTO<int>> Handle(CreateHSNMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.HSNMaster>(request);
            entity.IsActive = InventoryManagement.Domain.Common.BaseEntity.Status.Active;
            entity.IsDeleted = InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted;
           
            var newId = await _iHSNMasterCommandRepository.CreateAsync(entity);
            
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "HSN_CREATE",
                actionName: request.HSNCode,
                details: $"HSN Master '{request.HSNCode}' created successfully with Id {newId}.",
                module: "HSNMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);
           
            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "HSN Master created successfully.",
                Data = newId
            };
        }
    }
}