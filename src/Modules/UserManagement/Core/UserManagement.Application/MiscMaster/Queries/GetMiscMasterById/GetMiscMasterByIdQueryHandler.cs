using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQueryHandler : IRequestHandler<GetMiscMasterByIdQuery, GetMiscMasterDto>
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

            if (result == null)
                return null;

            var misctypemaster = _mapper.Map<GetMiscMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"MiscMaster details {misctypemaster.Id} was fetched.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return misctypemaster;
        }
        
    }
}