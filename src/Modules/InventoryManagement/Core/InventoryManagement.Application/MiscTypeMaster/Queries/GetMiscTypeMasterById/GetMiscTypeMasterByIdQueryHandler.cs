using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQueryHandler : IRequestHandler<GetMiscTypeMasterByIdQuery, ApiResponseDTO<GetMiscTypeMasterDto>>
    {
        private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscTypeMasterByIdQueryHandler(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<GetMiscTypeMasterDto>> Handle(GetMiscTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {

            var result = await _miscTypeMasterQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
            {
                return new ApiResponseDTO<GetMiscTypeMasterDto>
                {
                    IsSuccess = false,
                    Message = $"MiscTypeMaster with Id {request.Id} not found.",
                    Data = null
                };
            }

            var misctypemaster = _mapper.Map<GetMiscTypeMasterDto>(result);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"MiscTypeMaster details {misctypemaster.Id} was fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<GetMiscTypeMasterDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = misctypemaster
            };
        }
    }
}