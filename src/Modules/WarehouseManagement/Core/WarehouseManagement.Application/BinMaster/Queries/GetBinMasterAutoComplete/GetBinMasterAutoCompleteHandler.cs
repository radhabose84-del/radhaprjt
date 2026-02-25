using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete
{
    public class GetBinMasterAutoCompleteHandler : IRequestHandler<GetBinMasterAutoComplete, List<BinAutoDto>>
    {
        private readonly IBinMasterQueryRepository _binMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetBinMasterAutoCompleteHandler(IBinMasterQueryRepository binMasterQueryRepository, IMediator mediator, IMapper mapper)
        {
            _binMasterQueryRepository = binMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<BinAutoDto>> Handle(GetBinMasterAutoComplete request, CancellationToken cancellationToken)
            {
               var term = (request.SearchPattern ?? string.Empty).Trim();

                var result = await _binMasterQueryRepository.AutocompleteAsync(
                    term, request.Top, request.WarehouseId, request.RackId, cancellationToken);

                await _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode:   "GetBinMasterAutoComplete",
                    actionName:   result.Count.ToString(),
                    details:      $"BinMaster autocomplete fetched for term '{term}'.",
                    module:       "BinMaster"), cancellationToken);

                return result.ToList();
            }
        
       

    }
}