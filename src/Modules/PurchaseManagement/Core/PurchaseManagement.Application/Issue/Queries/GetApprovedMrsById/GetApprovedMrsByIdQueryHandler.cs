using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById
{
    public class GetApprovedMrsByIdQueryHandler : IRequestHandler<GetApprovedMrsByIdQuery, List<GetApprovedMrsByIdDto>>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetApprovedMrsByIdQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator)
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetApprovedMrsByIdDto>> Handle(GetApprovedMrsByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iissueQueryCommandRepository.GetApprovedMrsDetails(request.SearchPattern ?? string.Empty);
            var mrsapproved = _mapper.Map<List<GetApprovedMrsByIdDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetApprovedMrsByIdQuery",        
                    actionName: mrsapproved.Count.ToString(),
                    details: $"Approved MRS details was fetched.",
                    module:"MRS"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return mrsapproved;
        }
    }
}