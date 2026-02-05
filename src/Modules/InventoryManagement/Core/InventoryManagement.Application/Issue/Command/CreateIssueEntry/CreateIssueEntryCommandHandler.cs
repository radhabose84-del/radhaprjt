// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using InventoryManagement.Application.Common.Interfaces;
// using InventoryManagement.Application.Common.Interfaces.IIssue;
// using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
// using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
// using InventoryManagement.Domain.Common;
// using InventoryManagement.Domain.Entities.Issue;
// using InventoryManagement.Domain.Entities.Stock;
// using InventoryManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace InventoryManagement.Application.Issue.Command.CreateIssueEntry
// {
//     public class CreateIssueEntryCommandHandler : IRequestHandler<CreateIssueEntryCommand, int>
//     {

//         private readonly IIssueEntryCommandRepository _iissueEntryCommandRepository;
//         private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly ILogger<CreateIssueEntryCommandHandler> _logger;
//         private  readonly IPutAwayRuleQueryRepository _iputAwayRuleQueryRepository;

//         public CreateIssueEntryCommandHandler(IIssueEntryCommandRepository iissueEntryCommandRepository,
//             IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator,
//             IIPAddressService ipAddressService, ILogger<CreateIssueEntryCommandHandler> logger, IPutAwayRuleQueryRepository iputAwayRuleQueryRepository)
//         {
//             _iissueEntryCommandRepository = iissueEntryCommandRepository;
//             _iissueQueryCommandRepository = iissueQueryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _ipAddressService = ipAddressService;
//             _logger = logger;
//             _iputAwayRuleQueryRepository = iputAwayRuleQueryRepository;
//         }
        
//            public async Task<int> Handle(CreateIssueEntryCommand request, CancellationToken cancellationToken)
//                 {
//                     var issueEntryHeader = _mapper.Map<IssueHeader>(request.IssueEntry);

//                     // ✅ Auto-generate IssueNo if not set
//                     if (string.IsNullOrWhiteSpace(issueEntryHeader.IssueNo))
//                     {
//                         issueEntryHeader.IssueNo = await _iissueEntryCommandRepository.GenerateNextCodeAsync();
//                         issueEntryHeader.IssueDate = DateTime.Today;
//                         issueEntryHeader.IssuedBy = _ipAddressService.GetUserId();
//                         issueEntryHeader.IssuedDate = DateTime.Now;
//                         issueEntryHeader.IssuedByName = _ipAddressService.GetUserName();
//                         issueEntryHeader.IssuedIp = _ipAddressService.GetSystemIPAddress();
//                     }

//                     var stockLedgerEntries = new List<StockLedger>();
//                     var subStoreLedgerEntries = new List<SubStoreStockLedger>();

//                     // ✅ Get Misc Description (used to identify SubStores)
//                     var miscDescription = await _iissueQueryCommandRepository
//                         .GetDescriptionByIdAsync(request.IssueEntry.RequestCategoryId);

//                     List<GetPutAwayRuleItemIdDto> putawayRules = new();

//                     // ✅ Step 1: GRPC call only if category is SubStores
//                     if (string.Equals(miscDescription, MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase))
//                     {
//                         var itemIds = request.IssueEntry.IssueDetails.Select(x => x.ItemId).ToList();
//                         var warehouseIds = new List<int> { request.IssueEntry.SubStoresWarehouseId ?? 0 };

//                         putawayRules = await _iputAwayRuleQueryRepository
//                             .GetPutAwayRuleDetailsAsync(itemIds, warehouseIds) ;
//                     }

//                     // ✅ Step 2: Process all issue details
//                     foreach (var detail in request.IssueEntry.IssueDetails)
//                     {
//                         // ✅ STOCK LEDGER — StorageTypeId & TargetId from DTO
//                         var stockLedger = new StockLedger
//                         {
//                             UnitId = issueEntryHeader.UnitId,
//                             DocType = "ISS",
//                             DocSlNo = detail.Sno,
//                             DocDate = DateTime.Today,
//                             ItemId = detail.ItemId,
//                             UomId = detail.UomId,
//                             WarehouseId = detail.WarehouseStockId,
//                             StorageTypeId = detail.StorageTypeId,   // ✅ from DTO
//                             TargetId = detail.TargetId,             // ✅ from DTO
//                             ReceivedQty = 0,
//                             ReceivedValue = 0,
//                             IssueQty = detail.IssueQuantity ?? 0,
//                             IssueValue = (detail.IssueQuantity ?? 0) * detail.AvgRate
//                         };
//                         stockLedgerEntries.Add(stockLedger);

//                         // ✅ SUBSTORE LEDGER — StorageTypeId & TargetId from GRPC
//                         if (string.Equals(miscDescription, MiscEnumEntity.SubStores, StringComparison.OrdinalIgnoreCase))
//                         {
//                             var rule = putawayRules.FirstOrDefault(r => r.ItemId == detail.ItemId);

//                             var subStoreLedger = new SubStoreStockLedger
//                             {
//                                 UnitId = issueEntryHeader.UnitId,
//                                 DocType = "REC",
//                                 DocSlNo = detail.Sno,
//                                 DocDate = DateTime.Today,
//                                 DepartmentId = request.IssueEntry.DepartmentId,
//                                 ItemId = detail.ItemId,
//                                 UomId = detail.UomId,
//                                 WarehouseId = issueEntryHeader.SubStoresWarehouseId??0,
//                                 StorageTypeId = rule?.StorageTypeId ?? 0,  // ✅ from GRPC
//                                 TargetId = rule?.TargetId ?? 0,            // ✅ from GRPC
//                                 ReceivedQty = detail.IssueQuantity ?? 0,
//                                 ReceivedValue = (detail.IssueQuantity ?? 0) * detail.AvgRate,
//                                 IssueQty = 0,
//                                 IssueValue = 0
//                             };

//                             subStoreLedgerEntries.Add(subStoreLedger);
//                         }
//                     }

//                     // ✅ Step 3: Save all in one transaction
//                     var result = await _iissueEntryCommandRepository.CreateIssueWithLedgersAsync(
//                         issueEntryHeader,
//                         stockLedgerEntries,
//                         subStoreLedgerEntries,
//                         async () =>
//                         {
//                             // Publish Audit Log
//                             var domainEvent = new AuditLogsDomainEvent(
//                                 actionDetail: "Create",
//                                 actionCode: issueEntryHeader.Id.ToString(),
//                                 actionName: "Issue Entry Created",
//                                 details: "Issue, StockLedger, and optional SubStoreStockLedger created successfully",
//                                 module: "Issue"
//                             );

//                             await _mediator.Publish(domainEvent, cancellationToken);
//                         });

//                     return result;
//                 }

       
//     }
// }