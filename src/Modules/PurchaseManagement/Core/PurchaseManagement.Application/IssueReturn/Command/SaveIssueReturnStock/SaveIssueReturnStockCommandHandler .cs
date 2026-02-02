// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IWarehouse;
// using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.GRN.StockLedger;
// using PurchaseManagement.Domain.Entities.MRS;
// using MediatR;
// using Microsoft.AspNetCore.Http;

// namespace PurchaseManagement.Application.IssueReturn.Command.SaveIssueReturnStock
// {
//     public class SaveIssueReturnStockCommandHandler : IRequestHandler<SaveIssueReturnStockCommand, bool>
//     {
//         private readonly IIssueReturnEntryQueryRepository _issueReturnEntryQueryRepository;
//         private readonly IIssueReturnEntryCommandRepository _issueReturnEntryCommandRepository;
//         private readonly IWarehouseGrpcClient _warehouseGrpcClient;
//         private readonly IPutawayRuleGrpcClient _putawayRuleGrpcClient;
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public SaveIssueReturnStockCommandHandler(IIssueReturnEntryQueryRepository issueReturnEntryQueryRepository,
//             IIssueReturnEntryCommandRepository issueReturnEntryCommandRepository,
//             IWarehouseGrpcClient warehouseGrpcClient, IPutawayRuleGrpcClient putawayRuleGrpcClient)
//         {
//             _issueReturnEntryQueryRepository = issueReturnEntryQueryRepository;
//             _issueReturnEntryCommandRepository = issueReturnEntryCommandRepository;
//             _warehouseGrpcClient = warehouseGrpcClient;
//             _putawayRuleGrpcClient = putawayRuleGrpcClient;
//             _httpContextAccessor = new HttpContextAccessor();

//         }

//         public async Task<bool> Handle(SaveIssueReturnStockCommand request, CancellationToken ct)
//         {
//             var updated = await _issueReturnEntryQueryRepository.GetByIdWithDetails(request.IssueReturnHeaderId);
//             var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

//             if (updated == null)
//                 return false;

//             // --------------------------------------------
//             // 1️⃣ CONSUMPTION FLOW
//             // --------------------------------------------
//             if (updated.RequestCategoryName.Equals(MiscEnumEntity.Consumption, StringComparison.OrdinalIgnoreCase) &&
//                 updated.StatusName.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
//             {
//                 var approvedLines = updated.getIssueReturnDetails
//                     .Where(x => x.StatusName == MiscEnumEntity.Approved)
//                     .ToList();

//                 if (approvedLines.Any())
//                 {
//                     var itemIds = approvedLines.Select(x => x.ItemId).Distinct().ToList();
//                     var whIds = approvedLines.Select(x => x.WarehouseStockId).Distinct().ToList();

//                     var rules = await _putawayRuleGrpcClient
//                         .GetPutAwayRuleDetailsByWarehouseAsync(itemIds, whIds, ct);

//                     foreach (var line in approvedLines)
//                     {
//                         var rule = rules.FirstOrDefault(r => r.ItemId == line.ItemId &&
//                                                              r.WarehouseId == line.WarehouseStockId);

//                         var ledger = new StockLedger
//                         {
//                             UnitId = updated.UnitId,
//                             DocType = "RET",
//                             DocNo = updated.Id,
//                             DocSlNo = line.Id,
//                             DocDate = DateTime.Today,
//                             ItemId = line.ItemId,
//                             UomId = line.UomId,
//                             WarehouseId = line.WarehouseStockId,
//                             StorageTypeId = rule?.StorageTypeId ?? 0,
//                             TargetId = rule?.TargetId ?? 0,
//                             ReceivedQty = line.ReturnQuantity,
//                             ReceivedValue = line.ReturnValue,
//                             IssueQty = 0,
//                             IssueValue = 0
//                         };

//                         await _issueReturnEntryCommandRepository.InsertAsync(ledger);
//                     }
//                 }
//             }

//             // --------------------------------------------
//             // 2️⃣ SUBSTORE FLOW
//             // --------------------------------------------
//             else if (updated.RequestCategoryName.Equals(MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase) &&
//                      updated.StatusName.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
//             {
//                 var approvedLines = updated.getIssueReturnDetails
//                     .Where(x => x.StatusName == MiscEnumEntity.Approved)
//                     .ToList();

//                 if (approvedLines.Any())
//                 {
//                     var itemIds = approvedLines.Select(x => x.ItemId).Distinct().ToList();

//                     // collect parent warehouses
//                     var parentIds = new List<int>();

//                     foreach (var line in approvedLines)
//                     {
//                         var wh = await _warehouseGrpcClient.GetByIdAsync(line.WarehouseStockId, ct);
//                         if (wh != null) parentIds.Add(wh.ParentWarehouseId);
//                     }

//                     var putawayRules = await _putawayRuleGrpcClient
//                         .GetPutAwayRuleDetailsByWarehouseAsync(itemIds, parentIds, ct);

//                     foreach (var line in approvedLines)
//                     {
//                         var wh = await _warehouseGrpcClient.GetByIdAsync(line.WarehouseStockId, ct);
//                         int parentWhId = wh?.ParentWarehouseId ?? 0;

//                         var rule = putawayRules
//                             .FirstOrDefault(r => r.ItemId == line.ItemId &&
//                                                  r.WarehouseId == parentWhId);

//                         var ledger = new StockLedger
//                         {
//                             UnitId = updated.UnitId,
//                             DocType = "RET",
//                             DocNo = updated.Id,
//                             DocSlNo = line.Id,
//                             DocDate = DateTime.Today,
//                             ItemId = line.ItemId,
//                             UomId = line.UomId,
//                             WarehouseId = parentWhId,
//                             StorageTypeId = rule?.StorageTypeId ?? 0,
//                             TargetId = rule?.TargetId ?? 0,
//                             ReceivedQty = line.ReturnQuantity,
//                             ReceivedValue = line.ReturnValue,
//                             IssueQty = 0,
//                             IssueValue = 0
//                         };

//                         var subStoreLedger = new SubStoreStockLedger
//                         {
//                             UnitId = updated.UnitId,
//                             DocType = "RTM",
//                             DocNo = updated.Id,
//                             DocSlNo = line.Id,
//                             DocDate = DateTime.Today,
//                             ItemId = line.ItemId,
//                             UomId = line.UomId,
//                             WarehouseId = line.WarehouseStockId,
//                             StorageTypeId = line.StorageTypeId,
//                             TargetId = line.TargetId,
//                             IssueQty = line.ReturnQuantity,
//                             IssueValue = line.ReturnValue
//                         };

//                         await _issueReturnEntryCommandRepository.InsertStockAsync(ledger, subStoreLedger);
//                     }
//                 }
//             }

//             return true;
//         }
//     }
// }