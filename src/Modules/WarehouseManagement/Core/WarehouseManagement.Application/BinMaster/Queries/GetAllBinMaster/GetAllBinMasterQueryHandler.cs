using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IInvetoryManagement;
using WarehouseManagement.Application.Common.HttpResponse;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using UserManagement.Domain.Events;

namespace WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster
{
    public class GetAllBinMasterQueryHandler : IRequestHandler<GetAllBinMasterQuery, ApiResponseDTO<List<BinMasterDto>>>
    {

        private readonly IBinMasterQueryRepository _binMasterQueryRepository;
        private readonly IWarehouseMasterQueryRepository _warehouseRepo;  // if you have it
        private readonly IRackMasterQueryRepository _rackRepo;            // if you have it
        private readonly IUOMLookup _uomLookup;
        private readonly IMediator _mediator;

        public GetAllBinMasterQueryHandler(IBinMasterQueryRepository binRepo, IWarehouseMasterQueryRepository warehouseRepo, IRackMasterQueryRepository rackRepo, IUOMLookup uomLookup, IMediator mediator)
        {
            _binMasterQueryRepository = binRepo;
            _warehouseRepo = warehouseRepo;
            _rackRepo = rackRepo;
            _uomLookup = uomLookup;
            _mediator = mediator;
        }

      
        public async Task<ApiResponseDTO<List<BinMasterDto>>> Handle(GetAllBinMasterQuery request, CancellationToken cancellationToken)
        {
            // 1) Fetch page from DB
            var (bins, totalCount) = await _binMasterQueryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm ?? string.Empty
            );

            if (bins.Count == 0)
            {
                return new ApiResponseDTO<List<BinMasterDto>>
                {
                    IsSuccess = true,
                    Message = "No records found",
                    Data = bins,
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            // 2) Collect distinct keys for enrichment
            var warehouseIds = bins.Select(b => b.WarehouseId).Distinct().ToList();
            var rackIds      = bins.Select(b => b.RackId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var uomIds       = bins.Select(b => b.CapacityUOMId).Distinct().ToList();

            // 3) Kick off parallel fetches (swap to your actual clients/repos)
            var uomTask       = _uomLookup.GetAllAsync(); // returns all UOMs; filter later
            var warehousesTask= _warehouseRepo.GetwarehouseAsync(); // implement a light lookup (Id, Code, Name)
            var racksTask     = rackIds.Count > 0 
                                ? _rackRepo.GetRackAsync() 
                                : Task.FromResult(new List<RackMasterDto>());

            await Task.WhenAll(uomTask, warehousesTask, racksTask);

            // 4) Build lookup dictionaries
            var uomDict        = uomTask.Result.Where(u => uomIds.Contains(u.Id)).ToDictionary(x => x.Id, x => x.UOMName);
            var warehouseDict  = warehousesTask.Result.ToDictionary(x => x.Id, x => x.WarehouseName);
            var rackDictById   = racksTask.Result.ToDictionary(x => x.Id, x => x);

            // 5) Enrich rows
            foreach (var b in bins)
            {
                if (warehouseDict.TryGetValue(b.WarehouseId, out var whName))
                    b.WarehouseName = whName;

                if (b.RackId.HasValue && rackDictById.TryGetValue(b.RackId.Value, out var rack))
                {
                    b.RackCode = rack.RackCode;
                    b.RackName = rack.RackName;
                }

                if (uomDict.TryGetValue(b.CapacityUOMId, out var uomName))
                    b.CapacityUOMName = uomName;
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                   actionDetail: "GetAll",
                   actionCode:   "GetAllBinMaster",
                   actionName:   totalCount.ToString(),
                   details:      $"BinMaster fetched. Total Records: {totalCount}",
                   module:       "BinMaster"), cancellationToken);

            // 6) Wrap response
            return new ApiResponseDTO<List<BinMasterDto>>
            {
                IsSuccess   = true,
                Message     = "Fetched successfully",
                Data        = bins,
                TotalCount  = totalCount,
                PageNumber  = request.PageNumber,
                PageSize    = request.PageSize
            };
        }
        
    }
}