#nullable disable
using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetPendingQty
{
    public class GetPendingQtyQueryHandler :  IRequestHandler<GetPendingQtyQuery,GetPendingQtyDto>
    {
        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IMRSQueryRepository _imRSQueryRepository;  

        public GetPendingQtyQueryHandler(IMRSQueryRepository imRSQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imRSQueryRepository = imRSQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GetPendingQtyDto> Handle(GetPendingQtyQuery request, CancellationToken cancellationToken)
        {
            var result = await _imRSQueryRepository.GetPendingIssueAsync(request.OldUnitcode, request.ItemCode);

          

            var pendingQtyDto = _mapper.Map<GetPendingQtyDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingQty",
                actionCode: "GetPendingQtyQuery",
                actionName: request.ItemCode,
                details: $"PendingQty for ItemCode {request.ItemCode} in UnitCode {request.OldUnitcode} was fetched.",
                module: "IssueRequestPending"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return  pendingQtyDto;
        }
    }
}