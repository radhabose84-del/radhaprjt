using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Port.Queries.GetAllPorts;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.Purchase.PortMaster.Handlers;

public sealed class GetAllPortsQueryHandler : IRequestHandler<GetAllPortsQuery, PagedResult<PortMasterDto>>
{
    private readonly IPortMasterQueryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ICountryLookup _countryLookup; // ✅ lookup    
    public GetAllPortsQueryHandler(IPortMasterQueryRepository repo,
            IMapper mapper,
            IMediator mediator
            , ICountryLookup countryLookup            
            )
    {
        _repo = repo;
        _mapper = mapper;
        _mediator = mediator;
        _countryLookup = countryLookup;        
    }

    public async Task<PagedResult<PortMasterDto>> Handle(GetAllPortsQuery request, CancellationToken ct)
    {
         // 1) get paged rows from Purchase DB
        var (rows, total) = await _repo.GetAllAsync(
            page: request.PageNumber,
            size: request.PageSize,
            search: request.Search,
            countryId: request.CountryId,
            portTypeId: request.PortTypeId,
            ct);

        // 2) map to DTO
        var ports = _mapper.Map<List<PortMasterDto>>(rows) ?? new List<PortMasterDto>();

        // 3) enrich: Country name via lookup
        if (ports.Count > 0)
        {
            var countryIds = ports
                .Select(x => x.CountryId)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (countryIds.Length > 0)
            {
                var countries = await _countryLookup.GetByIdsAsync(countryIds, ct);

                var countryMap = countries
                    .Where(c => c != null)
                    .ToDictionary(c => c.CountryId, c => c.CountryName);

                foreach (var p in ports)
                {
                    if (countryMap.TryGetValue(p.CountryId, out var cname))
                        p.Country = cname;
                }
            }
        }
              

        // 4) audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetPortsQuery",
            actionCode: "Get",
            actionName: ports.Count.ToString(),
            details: "Port master list fetched.",
            module: "PortMaster"
        );
        await _mediator.Publish(ev, ct);

        // 5) response
        return new PagedResult<PortMasterDto> { Items = rows, Total = total };

    }
}
