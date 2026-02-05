using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete
{
    public class GetHSNMasterAutoCompleteQueryHandler : IRequestHandler<GetHSNMasterAutoCompleteQuery, List<GetHSNMasterAutoCompleteDto>>
    {
        private readonly IHSNMasterQueryRepository _hSNMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetHSNMasterAutoCompleteQueryHandler(IHSNMasterQueryRepository hSNMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _hSNMasterQueryRepository = hSNMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetHSNMasterAutoCompleteDto>> Handle(GetHSNMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var hsnData = await _hSNMasterQueryRepository.GetHSNMasterAutoCompleteAsync(request.SearchPattern ?? string.Empty , request.TypeCode ?? string.Empty);
            var result = _mapper.Map<List<GetHSNMasterAutoCompleteDto>>(hsnData);

            // Domain Event for Audit
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetHSNMasterAutoCompleteQueryHandler",
                actionName: result.Count.ToString(),
                details: $"HSN Master autocomplete fetched.",
                module: "HSNMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
        
    }
}