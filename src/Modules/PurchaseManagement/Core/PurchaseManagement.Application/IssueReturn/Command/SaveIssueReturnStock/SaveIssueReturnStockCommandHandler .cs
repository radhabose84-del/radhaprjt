using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.MRS;
using MediatR;
using Microsoft.AspNetCore.Http;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.IssueReturn.Command.SaveIssueReturnStock
{
    public class SaveIssueReturnStockCommandHandler : IRequestHandler<SaveIssueReturnStockCommand, bool>
    {
        private readonly IIssueReturnEntryQueryRepository _issueReturnEntryQueryRepository;
        private readonly IIssueReturnEntryCommandRepository _issueReturnEntryCommandRepository;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IPutawayRuleLookup _putawayRuleLookup;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveIssueReturnStockCommandHandler(IIssueReturnEntryQueryRepository issueReturnEntryQueryRepository,
            IIssueReturnEntryCommandRepository issueReturnEntryCommandRepository,
            IWarehouseLookup warehouseLookup, IPutawayRuleLookup putawayRuleLookup)
        {
            _issueReturnEntryQueryRepository = issueReturnEntryQueryRepository;
            _issueReturnEntryCommandRepository = issueReturnEntryCommandRepository;
            _warehouseLookup = warehouseLookup;
            _putawayRuleLookup = putawayRuleLookup;
            _httpContextAccessor = new HttpContextAccessor();

        }

        public async Task<bool> Handle(SaveIssueReturnStockCommand request, CancellationToken ct)
        {
            var updated = await _issueReturnEntryQueryRepository.GetByIdWithDetails(request.IssueReturnHeaderId);
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (updated == null)
                return false;
            var details = updated.getIssueReturnDetails;
            if (details == null)
                return true;

            // --------------------------------------------
            // 1️⃣ CONSUMPTION FLOW
            // --------------------------------------------
            if (string.Equals(updated.RequestCategoryName, MiscEnumEntity.Consumption, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(updated.StatusName, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
            {
                var approvedLines = details
                    .Where(x => x.StatusName == MiscEnumEntity.Approved)
                    .ToList();

                if (approvedLines.Any())
                {
                    var itemIds = approvedLines.Select(x => x.ItemId).Distinct().ToList();
                    var whIds = approvedLines.Select(x => x.WarehouseStockId).Distinct().ToList();

                    var rules = await _putawayRuleLookup
                        .GetPutAwayRuleDetailsByWarehouseAsync(itemIds, whIds, ct);

                    foreach (var line in approvedLines)
                    {
                        var rule = rules.FirstOrDefault(r => r.ItemId == line.ItemId &&
                                                             r.WarehouseId == line.WarehouseStockId);

                        var ledger = new StockLedger
                        {
                            UnitId = updated.UnitId,
                            DocType = "RET",
                            DocNo = updated.Id,
                            DocSlNo = line.Id,
                            DocDate = DateTime.Today,
                            ItemId = line.ItemId,
                            UomId = line.UomId,
                            WarehouseId = line.WarehouseStockId,
                            StorageTypeId = rule?.StorageTypeId ?? 0,
                            TargetId = rule?.TargetId ?? 0,
                            ReceivedQty = line.ReturnQuantity,
                            ReceivedValue = line.ReturnValue,
                            IssueQty = 0,
                            IssueValue = 0
                        };

                        await _issueReturnEntryCommandRepository.InsertAsync(ledger);
                    }
                }
            }

            // --------------------------------------------
            // 2️⃣ SUBSTORE FLOW
            // --------------------------------------------
            else if (string.Equals(updated.RequestCategoryName, MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(updated.StatusName, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
            {
                var approvedLines = details
                    .Where(x => x.StatusName == MiscEnumEntity.Approved)
                    .ToList();

                if (approvedLines.Any())
                {
                    var itemIds = approvedLines.Select(x => x.ItemId).Distinct().ToList();
                    var warehouseIds = approvedLines.Select(x => x.WarehouseStockId).Distinct().ToList();
                    var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds, ct);
                    var warehouseById = warehouses.ToDictionary(x => x.Id);

                    // collect parent warehouses
                    var parentIds = warehouses
                        .Where(x => x.ParentWarehouseId.HasValue)
                        .Select(x => x.ParentWarehouseId!.Value)
                        .Distinct()
                        .ToList();

                    var putawayRules = await _putawayRuleLookup
                        .GetPutAwayRuleDetailsByWarehouseAsync(itemIds, parentIds, ct);

                    foreach (var line in approvedLines)
                    {
                        warehouseById.TryGetValue(line.WarehouseStockId, out var wh);
                        int parentWhId = wh?.ParentWarehouseId ?? 0;

                        var rule = putawayRules
                            .FirstOrDefault(r => r.ItemId == line.ItemId &&
                                                 r.WarehouseId == parentWhId);

                        var ledger = new StockLedger
                        {
                            UnitId = updated.UnitId,
                            DocType = "RET",
                            DocNo = updated.Id,
                            DocSlNo = line.Id,
                            DocDate = DateTime.Today,
                            ItemId = line.ItemId,
                            UomId = line.UomId,
                            WarehouseId = parentWhId,
                            StorageTypeId = rule?.StorageTypeId ?? 0,
                            TargetId = rule?.TargetId ?? 0,
                            ReceivedQty = line.ReturnQuantity,
                            ReceivedValue = line.ReturnValue,
                            IssueQty = 0,
                            IssueValue = 0
                        };

                        var subStoreLedger = new SubStoreStockLedger
                        {
                            UnitId = updated.UnitId,
                            DocType = "RTM",
                            DocNo = updated.Id,
                            DocSlNo = line.Id,
                            DocDate = DateTime.Today,
                            ItemId = line.ItemId,
                            UomId = line.UomId,
                            WarehouseId = line.WarehouseStockId,
                            StorageTypeId = line.StorageTypeId,
                            TargetId = line.TargetId,
                            IssueQty = line.ReturnQuantity,
                            IssueValue = line.ReturnValue
                        };

                        await _issueReturnEntryCommandRepository.InsertStockAsync(ledger, subStoreLedger);
                    }
                }
            }

            return true;
        }
    }
}
