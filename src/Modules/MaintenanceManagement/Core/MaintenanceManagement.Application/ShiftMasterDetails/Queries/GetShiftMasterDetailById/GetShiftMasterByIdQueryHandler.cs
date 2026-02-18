using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById
{
    public class GetShiftMasterByIdQueryHandler : IRequestHandler<GetShiftMasterByIdQuery, ApiResponseDTO<ShiftMasterDetailByIdDto>>
    {
        private readonly IShiftMasterDetailQuery _shiftMasterQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetShiftMasterByIdQueryHandler(IShiftMasterDetailQuery shiftMasterQuery, IMapper mapper, IMediator mediator)
        {
            _shiftMasterQuery = shiftMasterQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<ShiftMasterDetailByIdDto>> Handle(GetShiftMasterByIdQuery request, CancellationToken cancellationToken)
        {
              var result = await _shiftMasterQuery.GetByIdAsync(request.Id);
            var shiftMaster = _mapper.Map<ShiftMasterDetailByIdDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Shift master details was fetched.",
                    module:"Shift master"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
              return new ApiResponseDTO<ShiftMasterDetailByIdDto> 
              { 
            IsSuccess = true, 
            Message = "Success", 
            Data = shiftMaster 
            };
        }
    }
}