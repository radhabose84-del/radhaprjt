using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete
{
    public class GetShiftMasterAutoCompleteQueryHandler : IRequestHandler<GetShiftMasterAutoCompleteQuery, ApiResponseDTO<List<ShiftMasterAutoCompleteDTO>>>
    {
        private readonly IShiftMasterQuery _shiftMasterQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetShiftMasterAutoCompleteQueryHandler(IShiftMasterQuery shiftMasterQuery, IMapper mapper, IMediator mediator)
        {
            _shiftMasterQuery = shiftMasterQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<ShiftMasterAutoCompleteDTO>>> Handle(GetShiftMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _shiftMasterQuery.GetShiftMaster(request.SearchPattern);
            var shiftMaster = _mapper.Map<List<ShiftMasterAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"Shift Master details was fetched.",
                    module:"Shift Master"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<ShiftMasterAutoCompleteDTO>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = shiftMaster 
             };   
        }
    }
}