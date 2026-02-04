using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterById
{
    public class GetHSNMasterByIdQueryHandler : IRequestHandler<GetHSNMasterByIdQuery, ApiResponseDTO<HSNMasterDto>>
    {
        private readonly IHSNMasterQueryRepository _iHSNMasterQueryRepository;
        private readonly IMapper _mapper;
          private readonly IMediator _mediator;

        public GetHSNMasterByIdQueryHandler(IHSNMasterQueryRepository hSNMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iHSNMasterQueryRepository = hSNMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

         public async Task<ApiResponseDTO<HSNMasterDto>> Handle(GetHSNMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iHSNMasterQueryRepository.GetByIdAsync(request.Id); // Entity return (not DTO)
            if (result == null)
            {
                return new ApiResponseDTO<HSNMasterDto>
                {
                    IsSuccess = false,
                    Message = $"HSN Master with Id {request.Id} not found.",
                    Data = null
                };
            }

            var hsnMaster = _mapper.Map<HSNMasterDto>(result);

            // Domain Audit Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"HSN Master details for ID {hsnMaster.Id} fetched.",
                module: "HSNMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<HSNMasterDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = hsnMaster
            };
        
        }
    }
}