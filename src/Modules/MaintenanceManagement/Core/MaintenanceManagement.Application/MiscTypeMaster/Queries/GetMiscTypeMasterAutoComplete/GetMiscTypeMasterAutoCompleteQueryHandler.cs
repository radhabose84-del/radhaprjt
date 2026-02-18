#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQueryHandler : IRequestHandler<GetMiscTypeMasterAutoCompleteQuery,ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
         private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscTypeMasterAutoCompleteQueryHandler(IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository, IMapper mapper, IMediator mediator)
         {
            _miscTypeMasterQueryRepository =miscTypeMasterQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
         }

        public  async Task<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>> Handle(GetMiscTypeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var miscTypeMasters  = await _miscTypeMasterQueryRepository.GetMiscTypeMaster(request.SearchPattern);

                    if (miscTypeMasters == null || !miscTypeMasters.Any())
            {
                return new ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>
                {
                    IsSuccess = false,
                    Message = $"No Misc Type Masters found matching '{request.SearchPattern}'.",
                    Data = new List<GetMiscTypeMasterAutocompleteDto>()
                };
            }

            var division = _mapper.Map<List<GetMiscTypeMasterAutocompleteDto>>(miscTypeMasters);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "", 
                    details: $"Division details was fetched.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>> { IsSuccess = true, Message = "Success", Data = division }; 
        }
    }
}