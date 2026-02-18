using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetail
{
    public class GetShiftMasterDetailQueryHandler : IRequestHandler<GetShiftMasterDetailQuery, ApiResponseDTO<List<ShiftMasterDetailDto>>>
    {
        private readonly IShiftMasterDetailQuery _shiftMasterQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetShiftMasterDetailQueryHandler(IShiftMasterDetailQuery shiftMasterQuery, IMapper mapper, IMediator mediator)
        {
            _shiftMasterQuery = shiftMasterQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<ShiftMasterDetailDto>>> Handle(GetShiftMasterDetailQuery request, CancellationToken cancellationToken)
        {
               var (shiftMaster, totalCount) = await _shiftMasterQuery.GetAllShiftMasterDetailAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var shiftMasterList = _mapper.Map<List<ShiftMasterDetailDto>>(shiftMaster);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetShiftMaster",
                    actionCode: "",        
                    actionName: "",
                    details: $"ShiftMaster details was fetched.",
                    module:"ShiftMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<ShiftMasterDetailDto>> 
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