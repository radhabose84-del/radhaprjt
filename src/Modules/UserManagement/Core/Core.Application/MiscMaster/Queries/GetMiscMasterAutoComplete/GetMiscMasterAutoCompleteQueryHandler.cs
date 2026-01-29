using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.Interfaces.IMiscMaster;
using Core.Application.MiscMaster.Queries.GetMiscMaster;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
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
            var miscTypeMasters  = await _miscMasterQueryRepository.GetMiscMaster(request.SearchPattern,request.MiscTypeCode);

           

            var division = _mapper.Map<List<GetMiscMasterAutoCompleteDto>>(miscTypeMasters);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "", 
                    details: $"Division details was fetched.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  division; 
        }
        
        
    }
}