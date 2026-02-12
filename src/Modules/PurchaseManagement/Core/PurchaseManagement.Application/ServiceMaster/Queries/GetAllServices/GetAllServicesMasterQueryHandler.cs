using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Common;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices
{
    public class GetAllServicesMasterQueryHandler : IRequestHandler<GetAllServicesMasterQuery, ApiResponse<List<GetServiceMasterDto>>>
    {


        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public readonly IServiceQueryRepository _serviceQueryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IHSNLookup   _hSNLookup;

        public GetAllServicesMasterQueryHandler(IMapper mapper, IMediator mediator, IServiceQueryRepository serviceMasterQueryRepository
            , IUOMLookup uomLookup, IHSNLookup hSNLookup)
        {
            _mapper = mapper;
            _mediator = mediator;
            _serviceQueryRepository = serviceMasterQueryRepository;
            _uomLookup = uomLookup;
            _hSNLookup = hSNLookup;

        }

        public async Task<ApiResponse<List<GetServiceMasterDto>>> Handle(GetAllServicesMasterQuery request, CancellationToken cancellationToken)
        {


            // Fetch paged data
            var (services, totalCount) = await _serviceQueryRepository.GetAllServiceMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);

           
            // Map to DTOs
            var dtoList = _mapper.Map<List<GetServiceMasterDto>>(services);
            
            var uomsTask = _uomLookup.GetAllAsync();
            var hsnsTask = _hSNLookup.GetAllAsync();

            await Task.WhenAll(uomsTask, hsnsTask);

            var uoms = await uomsTask;                  // assuming List<UomDto> (Id, Code, UOMName)
            var hsnList = await hsnsTask;

                    // 4) Build lookups (Id -> dto)
            var uomMap = uoms
                .GroupBy(u => u.Id)
                .ToDictionary(g => g.Key, g => g.First());

            var hsnMap = hsnList
                .GroupBy(h => h.Id)
                .ToDictionary(g => g.Key, g => g.First());;

            foreach (var item in dtoList)
            {

                  if (uomMap.TryGetValue(item.UomId, out var uom))
                    {
                        // choose a display string according to what your UOM dto exposes
                        item.UomName = uom.UOMName ?? uom.Code ?? item.UomName;
                    }
               // item.UomName = uomMap[item.UomId].UOMName ?? uomMap[item.UomId].Code ?? item.UomName;
                 if (hsnMap.TryGetValue(item.SacId, out var hsn))   
                    {
                     item.SacName = hsn.Description ?? hsn.HSNCode ?? item.SacName;
                    }
               

            }

            // Domain/Audit event (optional)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "ServiceMaster list fetched.",
                module: "ServiceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // Compose API response
            return new ApiResponse<List<GetServiceMasterDto>>(dtoList)
            {
                IsSuccess = true,
                Message = "Success",
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
