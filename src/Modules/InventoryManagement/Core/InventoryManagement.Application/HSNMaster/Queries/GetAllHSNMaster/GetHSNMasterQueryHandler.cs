using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster
{
    public class GetHSNMasterQueryHandler : IRequestHandler<GetHSNMasterQuery, ApiResponseDTO<List<HSNMasterDto>>>
    {
        private readonly IHSNMasterQueryRepository _iHSNMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetHSNMasterQueryHandler(IHSNMasterQueryRepository iHSNMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iHSNMasterQueryRepository = iHSNMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task<ApiResponseDTO<List<HSNMasterDto>>> Handle(GetHSNMasterQuery request, CancellationToken cancellationToken)
        {
            // Get paginated + searched data from DB
            var (hsnEntities, totalCount) = await _iHSNMasterQueryRepository.GetAllAsync( request.PageNumber, request.PageSize,request.SearchTerm);

            // Map to DTO
            var hsnList = _mapper.Map<List<HSNMasterDto>>(hsnEntities);

            // Domain Event for auditing
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "HSN Master data fetched",
                module: "HSNMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // Wrap response
            return new ApiResponseDTO<List<HSNMasterDto>>
            {
                IsSuccess = true,
                Message = "Fetched successfully",
                Data = hsnList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        
    }
        
    
}