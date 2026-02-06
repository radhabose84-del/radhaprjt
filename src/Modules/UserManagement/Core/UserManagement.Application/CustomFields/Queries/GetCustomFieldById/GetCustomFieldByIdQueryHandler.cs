using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.CustomFields.Queries.GetCustomFieldById
{
    public class GetCustomFieldByIdQueryHandler : IRequestHandler<GetCustomFieldByIdQuery, CustomFieldByIdDTO>
    {
        private readonly ICustomFieldQuery _customFieldQuery;
         private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetCustomFieldByIdQueryHandler(ICustomFieldQuery customFieldQuery, IMapper mapper, IMediator mediator)
        {
            _customFieldQuery = customFieldQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<CustomFieldByIdDTO> Handle(GetCustomFieldByIdQuery request, CancellationToken cancellationToken)
        {
             var (CustomField,CustomFieldMenu,CustomFieldUnit,CustomFieldOption) = await _customFieldQuery.GetByIdAsync(request.Id);
            var customField = _mapper.Map<CustomFieldByIdDTO>(CustomField);

            if (CustomFieldMenu != null)
             {
                 customField.Menu = _mapper.Map<List<CustomFieldMenuDto>>(CustomFieldMenu);
             }

             if (CustomFieldUnit != null)
             {
                 customField.Unit = _mapper.Map<List<CustomFieldUnitDto>>(CustomFieldUnit);
             }

             if (CustomFieldOption != null)
             {
                 customField.OptionalValues = _mapper.Map<List<CustomFieldOptionalValueDto>>(CustomFieldOption);
             }

          //Domain Event
                // var domainEvent = new AuditLogsDomainEvent(
                //     actionDetail: "GetById",
                //     actionCode: "",        
                //     actionName: "",
                //     details: $"Custom field details {customField.Id} was fetched.",
                //     module:"Custom Field"
                // );
                // await _mediator.Publish(domainEvent, cancellationToken);
          return  customField;
        }
    }
}