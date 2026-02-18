#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOM.Command.CreateUOM
{
    public class CreateUOMCommandHandler : IRequestHandler<CreateUOMCommand, ApiResponseDTO<UOMDto>>
    {
         private readonly IUOMCommandRepository _uomCommandRepository;
        private readonly IUOMQueryRepository _uomQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public CreateUOMCommandHandler(IUOMCommandRepository uomCommandRepository,IUOMQueryRepository uomQueryRepository,IMapper mapper,IMediator mediator)
        {
            _uomCommandRepository = uomCommandRepository;
            _uomQueryRepository = uomQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<UOMDto>> Handle(CreateUOMCommand request, CancellationToken cancellationToken)
        {
            var existingUOM = await _uomQueryRepository.GetByUOMNameAsync(request.UOMName);

               if (existingUOM != null)
               {
                   return new ApiResponseDTO<UOMDto>{IsSuccess = false, Message = "UOM already exists"};
               }
           
                 var uom  = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(request);

                var uomresult = await _uomCommandRepository.CreateAsync(uom);
                
                var locationMap = _mapper.Map<UOMDto>(uomresult);
                if (uomresult.Id > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: uomresult.Code,
                     actionName: uomresult.UOMName,
                     details: $"UOM '{uomresult.Code}' was created. UOMName: {uomresult.UOMName}",
                     module:"UOM"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return new ApiResponseDTO<UOMDto>{IsSuccess = true, Message = "UOM created successfully", Data = locationMap};
                }
               
                    return new ApiResponseDTO<UOMDto>{IsSuccess = false, Message = "UOM not created"};
        }
        
    }
}