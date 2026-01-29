using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ICustomField;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.CustomFields.Queries.GetCustomField
{
    public class GetCustomFieldQueryHandler : IRequestHandler<GetCustomFieldQuery, ApiResponseDTO<List<CustomFieldDTO>>>
    {
        private readonly ICustomFieldQuery _customFieldQuery;
         private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetCustomFieldQueryHandler(ICustomFieldQuery customFieldQuery, IMapper mapper, IMediator mediator)
        {
            _customFieldQuery = customFieldQuery;
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<ApiResponseDTO<List<CustomFieldDTO>>> Handle(GetCustomFieldQuery request, CancellationToken cancellationToken)
        {
             var (customfield, totalCount) = await _customFieldQuery.GetAllCustomFieldsAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var customfieldList = _mapper.Map<List<CustomFieldDTO>>(customfield);

             //Domain Event
                // var domainEvent = new AuditLogsDomainEvent(
                //     actionDetail: "GetCustomFields",
                //     actionCode: "",        
                //     actionName: "",
                //     details: $"Custom field details was fetched.",
                //     module:"Custom Field"
                // );

                // await _mediator.Publish(domainEvent, cancellationToken);
                
            return new ApiResponseDTO<List<CustomFieldDTO>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = customfieldList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}