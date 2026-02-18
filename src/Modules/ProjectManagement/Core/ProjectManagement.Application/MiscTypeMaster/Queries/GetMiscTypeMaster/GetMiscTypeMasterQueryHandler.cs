using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterQueryHandler   : IRequestHandler<GetMiscTypeMasterQuery,ApiResponseDTO<List<GetMiscTypeMasterDto>>>
    {
      private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
      private readonly IMapper _mapper;
      private readonly IMediator _mediator;

    public GetMiscTypeMasterQueryHandler(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
             _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
             _mapper =mapper;
             _mediator = mediator;
        }
      
        public async Task<ApiResponseDTO<List<GetMiscTypeMasterDto>>> Handle(GetMiscTypeMasterQuery request, CancellationToken cancellationToken)
        {
           var (misctype, totalCount) = await _miscTypeMasterQueryRepository.GetAllMiscTypeMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);             
            
            var MiscTypeMasterlist = _mapper.Map<List<GetMiscTypeMasterDto>>(misctype);
            
             //Domain Event
                 var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "GetAll",
                     actionCode: "",
                     actionName: "",
                     details: $"MiscTypeMaster details was fetched.",
                     module:"MiscTypeMaster"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
            var response =MiscTypeMasterlist.ToList();
            return new ApiResponseDTO<List<GetMiscTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = response,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
    
}