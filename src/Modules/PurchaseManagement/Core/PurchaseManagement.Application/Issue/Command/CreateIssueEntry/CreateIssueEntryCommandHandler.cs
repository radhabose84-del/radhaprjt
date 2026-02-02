// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Inventory;
// using Contracts.Dtos.Stock;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using PurchaseManagement.Application.Common.Exceptions;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IIssue;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.GRN.StockLedger;
// using PurchaseManagement.Domain.Entities.Issue;
// using PurchaseManagement.Domain.Entities.MRS;
// using PurchaseManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.Issue.Command.CreateIssueEntry
// {
//     public class CreateIssueEntryCommandHandler : IRequestHandler<CreateIssueEntryCommand, int>
//     {
//         private readonly IIssueEntryCommandRepository _iissueEntryCommandRepository;
//         private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IPutawayRuleGrpcClient _putawayRuleGrpcClient;
//         private readonly IStockLedgerGrpcClient _stockLedgerGrpcClient;
//         private readonly ILogger<CreateIssueEntryCommandHandler> _logger;

//         public CreateIssueEntryCommandHandler(IIssueEntryCommandRepository iissueEntryCommandRepository,
//             IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator,
//             IIPAddressService ipAddressService, IPutawayRuleGrpcClient putawayRuleGrpcClient,
//             IStockLedgerGrpcClient stockLedgerGrpcClient, ILogger<CreateIssueEntryCommandHandler> logger)
//         {
//             _iissueEntryCommandRepository = iissueEntryCommandRepository;
//             _iissueQueryCommandRepository = iissueQueryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _ipAddressService = ipAddressService;
//             _putawayRuleGrpcClient = putawayRuleGrpcClient;
//             _stockLedgerGrpcClient = stockLedgerGrpcClient;
//             _logger = logger;
//         }
        

//             public async Task<int> Handle(
//                 CreateIssueEntryCommand request,
//                 CancellationToken cancellationToken)
//             {
//                 // -----------------------------
//                 // HEADER
//                 // -----------------------------
//                 var issueEntryHeader = _mapper.Map<IssueHeader>(request.IssueEntry);

//                 if (string.IsNullOrWhiteSpace(issueEntryHeader.IssueNo))
//                 {
//                     issueEntryHeader.IssueNo = await _iissueEntryCommandRepository.GenerateNextCodeAsync();
//                     issueEntryHeader.IssueDate = DateTime.Today;
//                     issueEntryHeader.IssuedBy = _ipAddressService.GetUserId();
//                     issueEntryHeader.IssuedDate = DateTime.Now;
//                     issueEntryHeader.IssuedByName = _ipAddressService.GetUserName();
//                     issueEntryHeader.IssuedIp = _ipAddressService.GetSystemIPAddress();
//                 }

//                 // -----------------------------
//                 // CATEGORY CHECK
//                 // -----------------------------
//                 var miscDescription = await _iissueQueryCommandRepository
//                     .GetDescriptionByIdAsync(request.IssueEntry.RequestCategoryId);

//                 List<PutawayRuleDto> putawayRules = new();

//                 if (string.Equals(
//                     miscDescription,
//                     MiscEnumEntity.SubStores,
//                     StringComparison.OrdinalIgnoreCase))
//                 {
//                     var itemIds = request.IssueEntry.IssueDetails
//                         .Select(x => x.ItemId)
//                         .Distinct()
//                         .ToList();

//                     var warehouseIds = new List<int>
//                     {
//                         request.IssueEntry.SubStoresWarehouseId ?? 0
//                     };

//                     putawayRules = await _putawayRuleGrpcClient
//                         .GetPutAwayRuleDetailsByWarehouseAsync(
//                             itemIds,
//                             warehouseIds,
//                             cancellationToken);
//                 }

//                 int issueId;

//                 try
//                 {
//                     // -----------------------------
//                     // SAVE ISSUE HEADER ONLY
//                     // -----------------------------
//                     issueId = await _iissueEntryCommandRepository
//                         .CreateIssueAsync(issueEntryHeader);
//                 }
//                 catch (Exception ex)
//                 {
                    
//                     _logger.LogError(ex, "Issue header creation failed");
//                     throw;
//                 }

                

//                 // -----------------------------
//             // BUILD GRPC DTOs
//             // -----------------------------
//             var stockLedgerDtos = new List<StockLedgerDto>();
//                 var subStoreLedgerDtos = new List<SubStoreStockLedgerDto>();

//                 foreach (var detail in request.IssueEntry.IssueDetails)
//                 {
//                     // ---------- STOCK LEDGER (ISSUE) ----------
//                     stockLedgerDtos.Add(new StockLedgerDto
//                     {
//                         UnitId = issueEntryHeader.UnitId,
//                         DocType = "ISS",
//                         DocNo = issueId,
//                         DocSlNo = detail.Sno,
//                         DocDate = DateTime.Today,
//                         ItemId = detail.ItemId,
//                         UomId = detail.UomId,
//                         WarehouseId = detail.WarehouseStockId,
//                         StorageTypeId = detail.StorageTypeId,
//                         TargetId = detail.TargetId,
//                         ReceivedQty = 0,
//                         ReceivedValue = 0,
//                         IssueQty = detail.IssueQuantity ?? 0,
//                         IssueValue = (detail.IssueQuantity ?? 0) * detail.AvgRate
//                     });

//                     // ---------- SUBSTORE LEDGER (RECEIVE) ----------
//                     if (string.Equals(
//                         miscDescription,
//                         MiscEnumEntity.SubStores,
//                         StringComparison.OrdinalIgnoreCase))
//                     {
//                         var rule = putawayRules
//                             .FirstOrDefault(r => r.ItemId == detail.ItemId);

//                         subStoreLedgerDtos.Add(new SubStoreStockLedgerDto
//                         {
//                             UnitId = issueEntryHeader.UnitId,
//                             DocType = "REC",
//                             DocNo = issueId,
//                             DocSlNo = detail.Sno,
//                             DocDate = DateTime.Today,
//                             DepartmentId = request.IssueEntry.DepartmentId,
//                             ItemId = detail.ItemId,
//                             UomId = detail.UomId,
//                             WarehouseId = issueEntryHeader.SubStoresWarehouseId ?? 0,
//                             StorageTypeId = rule?.StorageTypeId ?? 0,
//                             TargetId = rule?.TargetId ?? 0,
//                             ReceivedQty = detail.IssueQuantity ?? 0,
//                             ReceivedValue = (detail.IssueQuantity ?? 0) * detail.AvgRate,
//                             IssueQty = 0,
//                             IssueValue = 0
//                         });
//                     }
//                 }

//                 // -----------------------------
//                 // GRPC INSERT
//                 // -----------------------------
//                 var stockResult = await _stockLedgerGrpcClient
//                     .InsertStockLedgerAsync(stockLedgerDtos, cancellationToken);

//                 if (!stockResult)
//                     throw new ApplicationException("StockLedger gRPC insert failed");

//                 if (subStoreLedgerDtos.Any())
//                 {
//                     var subStoreResult = await _stockLedgerGrpcClient
//                         .InsertSubStoreStockLedgerAsync(subStoreLedgerDtos, cancellationToken);

//                     if (!subStoreResult)
//                         throw new ApplicationException("SubStoreStockLedger gRPC insert failed");
//                 }

//                 // -----------------------------
//                 // AUDIT EVENT
//                 // -----------------------------
//                 await _mediator.Publish(
//                     new AuditLogsDomainEvent(
//                         "Create",
//                         issueId.ToString(),
//                         "Issue Entry Created",
//                         "Issue header saved and ledgers inserted via gRPC",
//                         "Issue"),
//                     cancellationToken);

//                 return issueId;
//             }

        
        

//     //    public async Task<int> Handle(CreateIssueEntryCommand request, CancellationToken cancellationToken)
//         //         {
//         //             var issueEntryHeader = _mapper.Map<IssueHeader>(request.IssueEntry);

//         //             // ✅ Auto-generate IssueNo if not set
//         //             if (string.IsNullOrWhiteSpace(issueEntryHeader.IssueNo))
//         //             {
//         //                 issueEntryHeader.IssueNo = await _iissueEntryCommandRepository.GenerateNextCodeAsync();
//         //                 issueEntryHeader.IssueDate = DateTime.Today;
//         //                 issueEntryHeader.IssuedBy = _ipAddressService.GetUserId();
//         //                 issueEntryHeader.IssuedDate = DateTime.Now;
//         //                 issueEntryHeader.IssuedByName = _ipAddressService.GetUserName();
//         //                 issueEntryHeader.IssuedIp = _ipAddressService.GetSystemIPAddress();
//         //             }

//         //             var stockLedgerEntries = new List<StockLedger>();
//         //             var subStoreLedgerEntries = new List<SubStoreStockLedger>();

//         //             // ✅ Get Misc Description (used to identify SubStores)
//         //             var miscDescription = await _iissueQueryCommandRepository
//         //                 .GetDescriptionByIdAsync(request.IssueEntry.RequestCategoryId);

//         //             List<PutawayRuleDto> putawayRules = new();

//         //             // ✅ Step 1: GRPC call only if category is SubStores
//         //             if (string.Equals(miscDescription, MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase))
//         //             {
//         //                 var itemIds = request.IssueEntry.IssueDetails.Select(x => x.ItemId).ToList();
//         //                 var warehouseIds = new List<int> { request.IssueEntry.SubStoresWarehouseId ?? 0 };

//         //                 putawayRules = await _putawayRuleGrpcClient
//         //                     .GetPutAwayRuleDetailsByWarehouseAsync(itemIds, warehouseIds, cancellationToken);
//         //             }

//         //             // ✅ Step 2: Process all issue details
//         //             foreach (var detail in request.IssueEntry.IssueDetails)
//         //             {
//         //                 // ✅ STOCK LEDGER — StorageTypeId & TargetId from DTO
//         //                 var stockLedger = new StockLedger
//         //                 {
//         //                     UnitId = issueEntryHeader.UnitId,
//         //                     DocType = "ISS",
//         //                     DocSlNo = detail.Sno,
//         //                     DocDate = DateTime.Today,
//         //                     ItemId = detail.ItemId,
//         //                     UomId = detail.UomId,
//         //                     WarehouseId = detail.WarehouseStockId,
//         //                     StorageTypeId = detail.StorageTypeId,   // ✅ from DTO
//         //                     TargetId = detail.TargetId,             // ✅ from DTO
//         //                     ReceivedQty = 0,
//         //                     ReceivedValue = 0,
//         //                     IssueQty = detail.IssueQuantity ?? 0,
//         //                     IssueValue = (detail.IssueQuantity ?? 0) * detail.AvgRate
//         //                 };
//         //                 stockLedgerEntries.Add(stockLedger);

//         //                 // ✅ SUBSTORE LEDGER — StorageTypeId & TargetId from GRPC
//         //                 if (string.Equals(miscDescription, MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase))
//         //                 {
//         //                     var rule = putawayRules.FirstOrDefault(r => r.ItemId == detail.ItemId);

//         //                     var subStoreLedger = new SubStoreStockLedger
//         //                     {
//         //                         UnitId = issueEntryHeader.UnitId,
//         //                         DocType = "REC",
//         //                         DocSlNo = detail.Sno,
//         //                         DocDate = DateTime.Today,
//         //                         DepartmentId = request.IssueEntry.DepartmentId,
//         //                         ItemId = detail.ItemId,
//         //                         UomId = detail.UomId,
//         //                         WarehouseId = issueEntryHeader.SubStoresWarehouseId??0,
//         //                         StorageTypeId = rule?.StorageTypeId ?? 0,  // ✅ from GRPC
//         //                         TargetId = rule?.TargetId ?? 0,            // ✅ from GRPC
//         //                         ReceivedQty = detail.IssueQuantity ?? 0,
//         //                         ReceivedValue = (detail.IssueQuantity ?? 0) * detail.AvgRate,
//         //                         IssueQty = 0,
//         //                         IssueValue = 0
//         //                     };

//         //                     subStoreLedgerEntries.Add(subStoreLedger);
//         //                 }
//         //             }

//         //             // ✅ Step 3: Save all in one transaction
//         //             var result = await _iissueEntryCommandRepository.CreateIssueWithLedgersAsync(
//         //                 issueEntryHeader,
//         //                 stockLedgerEntries,
//         //                 subStoreLedgerEntries,
//         //                 async () =>
//         //                 {
//         //                     // Publish Audit Log
//         //                     var domainEvent = new AuditLogsDomainEvent(
//         //                         actionDetail: "Create",
//         //                         actionCode: issueEntryHeader.Id.ToString(),
//         //                         actionName: "Issue Entry Created",
//         //                         details: "Issue, StockLedger, and optional SubStoreStockLedger created successfully",
//         //                         module: "Issue"
//         //                     );

//         //                     await _mediator.Publish(domainEvent, cancellationToken);
//         //                 });

//         //             return result;
//         //         }



//     }
// }

