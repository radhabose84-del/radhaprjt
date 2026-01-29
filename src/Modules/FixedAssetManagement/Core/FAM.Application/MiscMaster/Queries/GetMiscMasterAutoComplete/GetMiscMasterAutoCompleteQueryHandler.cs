using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQueryHandler : IRequestHandler<GetMiscMasterAutoCompleteQuery,List<GetMiscMasterAutoCompleteDto>>
    {
         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
     public GetMiscMasterAutoCompleteQueryHandler(IMiscMasterQueryRepository miscMasterQueryRepository, IMapper mapper, IMediator mediator)
         {
            _miscMasterQueryRepository =miscMasterQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
         }


          public  async Task<List<GetMiscMasterAutoCompleteDto>> Handle(GetMiscMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var miscTypeMasters  = await _miscMasterQueryRepository.GetMiscMaster( request.MiscTypeCode,request.MiscTypeName);

                    if (miscTypeMasters == null )
            {
                throw new ValidationException($"No Misc Type Masters found for TypeCode '{request.MiscTypeCode}' and TypeName '{request.MiscTypeName}'.");
              
            }

            var division = _mapper.Map<List<GetMiscMasterAutoCompleteDto>>(miscTypeMasters);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "", 
                    details: $"Misc Type details was fetched.",
                    module:"MiscType"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  division; 
        }
        
    }
}