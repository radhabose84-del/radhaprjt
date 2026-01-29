using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQueryHandler  : IRequestHandler<GetMiscMasterByIdQuery, GetMiscMasterDto>
    {

        private readonly IMiscMasterQueryRepository  _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

         public GetMiscMasterByIdQueryHandler(IMiscMasterQueryRepository miscMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
        } 

          public async  Task<GetMiscMasterDto> Handle(GetMiscMasterByIdQuery request, CancellationToken cancellationToken)
        {
                  
            var result = await _miscMasterQueryRepository.GetByIdAsync(request.Id);
          
           
            var misctypemaster = _mapper.Map<GetMiscMasterDto>(result);

            //Domain Event
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "GetById",
                        actionCode: "",        
                        actionName: "",
                        details: $"MiscTypeMaster details {misctypemaster.Id} was fetched.",
                        module:"MiscTypeMaster"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
            return  misctypemaster;
        }

        
    }
}