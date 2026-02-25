using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete
{
    public class GetServiceAutocompleteQueryHandler : IRequestHandler<GetServiceAutocompleteQuery, List<ServiceMasterAutoCompleteDto>>
    {

        private readonly IServiceQueryRepository _serviceQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUOMLookup _uomLookup;
        private readonly IHSNLookup   _hSNLookup;

        public GetServiceAutocompleteQueryHandler(IServiceQueryRepository serviceQueryRepository, IMapper mapper, IMediator mediator, IUOMLookup uomLookup, IHSNLookup hSNLookup)
        {
            _serviceQueryRepository = serviceQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _uomLookup = uomLookup;
            _hSNLookup = hSNLookup;
        }



        // public async Task<List<ServiceMasterAutoCompleteDto>> Handle(GetServiceAutocompleteQuery request, CancellationToken cancellationToken)
        // {



        //     var term = (request.SearchPattern ?? string.Empty).Trim();
        //     var serviceMasters = await _serviceQueryRepository.ServiceMasterAuotoComplete(term);

        //     var service = _mapper.Map<List<ServiceMasterAutoCompleteDto>>(serviceMasters);
        //     //Domain Event
        //     var domainEvent = new AuditLogsDomainEvent(
        //         actionDetail: "GetAll",
        //         actionCode: "",
        //         actionName: "",
        //         details: $"Service details was fetched.",
        //         module: "Service"
        //     );
        //     await _mediator.Publish(domainEvent, cancellationToken);
        //     return service;
        // }   
        
             public async Task<List<ServiceMasterAutoCompleteDto>> Handle(
            GetServiceAutocompleteQuery request,
            CancellationToken cancellationToken)
        {
            var term = (request.SearchPattern ?? string.Empty).Trim();

            // 1) Base data
            var serviceMasters = await _serviceQueryRepository.ServiceMasterAuotoComplete(term);
            var serviceDtos = _mapper.Map<List<ServiceMasterAutoCompleteDto>>(serviceMasters);

            if (serviceDtos.Count == 0)
            {
                await _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",
                    actionName: "",
                    details: "Service autocomplete returned 0 results.",
                    module: "Service"), cancellationToken);
                return serviceDtos;
            }

            // 2) Collect distinct IDs referenced by the results
            var neededUomIds = serviceDtos.Where(s => s.UomId > 0)
                                          .Select(s => s.UomId)
                                          .Distinct()
                                          .ToHashSet();

            var neededSacIds = serviceDtos.Where(s => s.SacId > 0)
                                          .Select(s => s.SacId)
                                          .Distinct()
                                          .ToHashSet();

            // 3) Fetch lookups in parallel (adjust if you have bulk-by-IDs APIs)
            var uomsTask = _uomLookup.GetAllAsync();              // List<UomDto> (Id, Code, UOMName)
            var hsnsTask = _hSNLookup.GetAllAsync();
            await Task.WhenAll(uomsTask, hsnsTask);

            var uoms = await uomsTask;
            var hsnList = await hsnsTask;

            // 4) Build maps only for needed IDs
            var uomMap = uoms.Where(u => neededUomIds.Contains(u.Id))
                             .GroupBy(u => u.Id)
                             .ToDictionary(g => g.Key, g => g.First());

            var hsnMap = hsnList.Where(h => neededSacIds.Contains(h.Id))
                                .GroupBy(h => h.Id)
                                .ToDictionary(g => g.Key, g => g.First());

            // 5) Enrich results using your property names (UomName, SacName)
            foreach (var item in serviceDtos)
            {
                if (item.UomId > 0 && uomMap.TryGetValue(item.UomId, out var uom))
                {
                    item.UomName = !string.IsNullOrWhiteSpace(uom.UOMName)
                        ? uom.UOMName
                        : (!string.IsNullOrWhiteSpace(uom.Code) ? uom.Code : item.UomName);
                }

                if (item.SacId > 0 && hsnMap.TryGetValue(item.SacId, out var hsn))
                {
                    item.SacName = !string.IsNullOrWhiteSpace(hsn.Description)
                        ? hsn.Description
                        : (!string.IsNullOrWhiteSpace(hsn.HSNCode) ? hsn.HSNCode : item.SacName);

                    item.GstPercentage =  hsn.GSTPercentage;
                   
                }
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Service autocomplete fetched with UOM & SAC enrichment.",
                module: "Service"), cancellationToken);

            return serviceDtos;
        }
    }
}

