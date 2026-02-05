using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Application.Common.HttpResponse;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries
{
    public class GetAssetUnitAutoCompleteQueryHandler : IRequestHandler<GetAssetUnitAutoCompleteQuery, List<AssetUnitAutoCompleteDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;  // ✅ lookup dependency

        public GetAssetUnitAutoCompleteQueryHandler(IMapper mapper, IMediator mediator,
            IUnitLookup unitLookup)  // ✅ inject lookup
        {
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
        }

        public async Task<List<AssetUnitAutoCompleteDto>> Handle(GetAssetUnitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            // ✅ Use lookup interface instead of direct database query
            var result = await _unitLookup.GetUserUnitByUserNameAsync(request.Username);
            var assetunits = result.Select(u => new AssetUnitAutoCompleteDto
            {
                OldUnitId = u.OldUnitId,
                UnitName = u.UnitName
            }).ToList();

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAll",
                actionName: "Assetunit",
                details: $"Assetunit details was fetched.",
                module: "Assetunit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return assetunits;
        }
    }
}