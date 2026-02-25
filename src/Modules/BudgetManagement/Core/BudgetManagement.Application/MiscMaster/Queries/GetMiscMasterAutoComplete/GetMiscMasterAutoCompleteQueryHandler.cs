using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
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
            var miscTypeMasters  = await _miscMasterQueryRepository.GetMiscMaster(request.SearchPattern ?? string.Empty, request.MiscTypeCode ?? string.Empty);

           

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