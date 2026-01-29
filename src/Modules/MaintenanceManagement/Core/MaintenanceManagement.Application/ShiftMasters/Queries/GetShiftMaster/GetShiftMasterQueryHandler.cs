using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster
{
    public class GetShiftMasterQueryHandler : IRequestHandler<GetShiftMasterQuery, ApiResponseDTO<List<ShiftMasterDTO>>>
    {
        private readonly IShiftMasterQuery _shiftMasterQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetShiftMasterQueryHandler(IShiftMasterQuery shiftMasterQuery, IMapper mapper, IMediator mediator)
        {
            _shiftMasterQuery = shiftMasterQuery;
            _mapper = mapper;
            _mediator = mediator;
            
        }
        public async Task<ApiResponseDTO<List<ShiftMasterDTO>>> Handle(GetShiftMasterQuery request, CancellationToken cancellationToken)
        {
              var (shiftMaster, totalCount) = await _shiftMasterQuery.GetAllShiftMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var shiftMasterList = _mapper.Map<List<ShiftMasterDTO>>(shiftMaster);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetShiftMaster",
                    actionCode: "",        
                    actionName: "",
                    details: $"ShiftMaster details was fetched.",
                    module:"ShiftMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<ShiftMasterDTO>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = shiftMasterList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}