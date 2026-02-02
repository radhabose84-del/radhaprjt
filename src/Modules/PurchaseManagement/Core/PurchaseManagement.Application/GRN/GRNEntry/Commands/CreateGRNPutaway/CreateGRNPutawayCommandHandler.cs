// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using PurchaseManagement.Application.Common.Exceptions;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
// using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
// using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
// using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
// using PurchaseManagement.Domain.Entities.GRN.StockLedger;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway
// {
//     public class CreateGRNPutawayCommandHandler : IRequestHandler<CreateGRNPutawayCommand, int>
//     {

//         private readonly IGRNEntryCommandRepository _iGrnEntryCommandRepository;
//         private readonly IGRNEntryQueryRepository _igrnEntryQueryRepository;
//         private readonly IPurchaseBillEntryCommandRepository _billCommandRepo;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IStockLedgerGrpcClient _stockLedgerGrpcClient;
//         public CreateGRNPutawayCommandHandler(IGRNEntryCommandRepository iGrnEntryCommandRepository, IMapper mapper, IMediator mediator, IGRNEntryQueryRepository igrnEntryQueryRepository, IIPAddressService ipAddressService, IStockLedgerGrpcClient stockLedgerGrpcClient)
//         {
//             _iGrnEntryCommandRepository = iGrnEntryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _igrnEntryQueryRepository = igrnEntryQueryRepository;
//             _ipAddressService = ipAddressService;
//             _stockLedgerGrpcClient = stockLedgerGrpcClient;
//         }
        
//         public async Task<int> Handle(
//                     CreateGRNPutawayCommand request,
//                     CancellationToken cancellationToken)
//                 {
//                     if (request.PutawayList == null || !request.PutawayList.Any())
//                         throw new ArgumentException("Putaway list cannot be empty.");

//                     // Map DTO → Domain
//                     var putawayList = _mapper.Map<List<GrnPutAwayRule>>(request.PutawayList);

//                     foreach (var item in putawayList)
//                     {
//                         item.PutAwayDate = DateTime.Now;
//                         item.CreatedBy = _ipAddressService.GetUserId();
//                         item.CreatedDate = DateTime.Now;
//                         item.CreatedByName = _ipAddressService.GetUserName();
//                         item.CreatedIP = _ipAddressService.GetSystemIPAddress();

//                         if (!item.ConversionFactor.HasValue || item.ConversionFactor.Value <= 0)
//                             item.ConversionFactor = 1;

//                         if (item.QcAcceptedQtyStockUom <= 0)
//                         {
//                             item.QcAcceptedQtyStockUom =
//                                 item.QcAcceptedQtyPurchaseUom * item.ConversionFactor.Value;
//                         }
//                     }

//                     // -------------------------------
//                     // Prepare StockLedger DTOs (gRPC)
//                     // -------------------------------
//                     var stockLedgerDtos = new List<Contracts.Dtos.Stock.StockLedgerDto>();

//                     foreach (var item in putawayList)
//                     {
//                         var unitPrice =
//                             await _igrnEntryQueryRepository.GetUnitPriceAsync(
//                                 item.PoId, item.ItemId, item.PoSlNoLocal)
//                             ?? 0;

//                         var purchaseQty =
//                             item.QcAcceptedQtyStockUom / item.ConversionFactor.Value;

//                         var receivedValue = purchaseQty * unitPrice;

//                         stockLedgerDtos.Add(new Contracts.Dtos.Stock.StockLedgerDto
//                         {
//                             UnitId = item.UnitId,
//                             DocType = "GRN",
//                             DocNo = item.GrnId,
//                             DocSlNo = item.PoSlNoLocal,
//                             DocDate = DateTime.Today,
//                             ItemId = item.ItemId,
//                             UomId = item.StockUomId,
//                             WarehouseId = item.WarehouseId,
//                             StorageTypeId = item.StorageTypeId,
//                             TargetId = item.TargetId,
//                             ReceivedQty = item.QcAcceptedQtyStockUom,
//                             ReceivedValue = receivedValue,
//                             IssueQty = 0,
//                             IssueValue = 0
//                         });
//                     }

//                     // ------------------------------------
//                     // Save Putaway (LOCAL DB TRANSACTION)
//                     // ------------------------------------
//                     var result =
//                         await _iGrnEntryCommandRepository.CreatePutawayListAsync(
//                             putawayList);

//                     if (result <= 0)
//                         throw new InvalidOperationException("Failed to create GRN Putaway.");

//                     // 🔹 Insert StockLedger via gRPC
//                     var ledgerInserted =
//                         await _stockLedgerGrpcClient.InsertStockLedgerAsync(
//                             stockLedgerDtos,
//                             cancellationToken);

//                     if (!ledgerInserted)
//                         throw new InvalidOperationException(
//                             "Failed to insert StockLedger via gRPC.");

//                     // 🔹 Audit logs
//                     foreach (var item in putawayList)
//                     {
//                         var domainEvent = new AuditLogsDomainEvent(
//                             actionDetail: "Create",
//                             actionCode: item.GrnId.ToString(),
//                             actionName: item.GrnDetailId.ToString(),
//                             details: "GrnPutaway & StockLedger created via gRPC",
//                             module: "GrnPutaway"
//                         );

//                         await _mediator.Publish(domainEvent, cancellationToken);
//                     }

//                     return result;

//                 }


// //         public async Task<int> Handle(CreateGRNPutawayCommand request, CancellationToken cancellationToken)
//         //         {
//         //             if (request.PutawayList == null || !request.PutawayList.Any())
//         //                     throw new ArgumentException("Putaway list cannot be empty.");

//         //                 // Map DTOs to domain entities
//         //                 var putawayList = _mapper.Map<List<GrnPutAwayRule>>(request.PutawayList);

//         //                 foreach (var item in putawayList)
//         //                 {
//         //                     item.PutAwayDate = DateTime.Now;
//         //                     item.CreatedBy = _ipAddressService.GetUserId();
//         //                     item.CreatedDate = DateTime.Now;
//         //                     item.CreatedByName = _ipAddressService.GetUserName();
//         //                     item.CreatedIP = _ipAddressService.GetSystemIPAddress();

//         //                     // Normalize conversion factor to avoid divide-by-zero
//         //                     if (!item.ConversionFactor.HasValue || item.ConversionFactor.Value <= 0)
//         //                         item.ConversionFactor = 1;

//         //                     // Auto compute stock qty only if not provided
//         //                     if (item.QcAcceptedQtyStockUom <= 0)
//         //                     {
//         //                         item.QcAcceptedQtyStockUom =
//         //                             item.QcAcceptedQtyPurchaseUom * item.ConversionFactor.Value;
//         //                     }
//         //                 }

//         //                 // Prepare Stock Ledger entries
//         //                 var stockLedgerEntries = new List<StockLedger>();

//         //                 foreach (var item in putawayList)
//         //                 {
//         //                     var unitPrice =
//         //                         await _igrnEntryQueryRepository.GetUnitPriceAsync(item.PoId, item.ItemId, item.PoSlNoLocal)
//         //                         ?? 0;

//         //                     // Purchase UOM Qty = Stock Qty / Conversion Factor
//         //                     var purchaseQty = item.QcAcceptedQtyStockUom / item.ConversionFactor.Value;

//         //                     // ReceivedValue = PurchaseQty * UnitPrice
//         //                     var receivedValue = purchaseQty * unitPrice;

//         //                     stockLedgerEntries.Add(new StockLedger
//         //                     {
//         //                         UnitId = item.UnitId,
//         //                         DocType = "GRN",
//         //                         DocNo = item.GrnId,
//         //                         DocSlNo = item.PoSlNoLocal,
//         //                         DocDate = DateTime.Today,
//         //                         ItemId = item.ItemId,
//         //                         UomId = item.StockUomId,
//         //                         WarehouseId = item.WarehouseId,
//         //                         StorageTypeId = item.StorageTypeId,
//         //                         TargetId = item.TargetId,
//         //                         ReceivedQty = item.QcAcceptedQtyStockUom,
//         //                         ReceivedValue = receivedValue,
//         //                         IssueQty = 0,
//         //                         IssueValue = 0
//         //                     });
//         //                 }

//         //                 // Save inside transaction
//         //                 var result = await _iGrnEntryCommandRepository.CreatePutawayWithStockLedgerAsync(
//         //                     putawayList,
//         //                     stockLedgerEntries,
//         //                     async () =>
//         //                     {
//         //                         // Publish audit logs
//         //                         foreach (var item in putawayList)
//         //                         {
//         //                             var domainEvent = new AuditLogsDomainEvent(
//         //                                 actionDetail: "Create",
//         //                                 actionCode: item.GrnId.ToString(),
//         //                                 actionName: item.GrnDetailId.ToString(),
//         //                                 details: "GrnPutaway & StockLedger entries created with received value",
//         //                                 module: "GrnPutaway"
//         //                             );

//         //                             await _mediator.Publish(domainEvent, cancellationToken);
//         //                         }
//         //                     });

//         //                 if (result <= 0)
//         //                     throw new InvalidOperationException("Failed to create GRN Putaway.");


//         //          /*    var grnId = putawayList.First().GrnId;

//         //            await _billCommandRepo.DeleteByGrnIdAsync(grnId, ct);


//         //             var billEntryId = await CreatePurchaseBillEntryFromGrnAsync(
//         //                 grnId,
//         //                 putawayList,
//         //                 cancellationToken);
//         //  */

//         //             return result;
//         //         }

//         private async Task<int> CreatePurchaseBillEntryFromGrnAsync(
//          int grnId,
//          List<GrnPutAwayRule> putawayList,
//          CancellationToken ct)
//         {
//             // 🔸 4.1: Get GRN header + details
//             var grn = await _iGrnEntryCommandRepository.GetGrnWithDetailsAsync(grnId, ct);
//             if (grn is null)
//                 throw new ArgumentException($"GRN with Id {grnId} not found.");

//             if (grn.GrnDetails == null || !grn.GrnDetails.Any())
//                 throw new InvalidOperationException("GRN has no details to create bill entry.");

//             // Only include GRN details that are actually in this putaway batch
//             var grnDetailIds = putawayList
//                 .Select(x => x.GrnDetailId)
//                 .Distinct()
//                 .ToHashSet();

//             var usedDetails = grn.GrnDetails
//                 .Where(d => grnDetailIds.Contains(d.Id))
//                 .ToList();

//             if (!usedDetails.Any())
//                 throw new InvalidOperationException("No matching GRN detail rows found for putaway lines.");

//             // Take PoId / Category / Method etc from first GRN detail
//             var firstDetail = usedDetails.First();

//             // 🔸 4.2: Build Bill Entry Header DTO
//             var headerDto = new PurchaseBillEntryHeaderDto
//             {
//                 // Unit comes from GRN
//                 UnitId = grn.UnitId,

//                 // BillNumber = InvoiceNo; fallback to GrnNo
//                 BillNumber = !string.IsNullOrWhiteSpace(grn.InvoiceNo)
//                     ? grn.InvoiceNo
//                     : grn.GrnNo,


//                 BillDate = DateOnly.FromDateTime(grn.InvoiceDate.LocalDateTime),

//                 PartyId = grn.PartyId,
//                 PoId = firstDetail.PoId,       // if your GrnDetail has PoId
//                 GrnId = grn.Id,

//                 POCategoryId = firstDetail.PoCategoryId,
//                 POMethodId = firstDetail.PoMethodId,

//                 // 🔹 Totals from GRN header (match your SQL columns)
//                 SubTotal = grn.ItemsTotal ?? 0m,       // ItemsTotal
//                 DiscountTotal = grn.DiscountTotal ?? 0m,
//                 TaxableAmount = grn.TaxableAmount ?? 0m,
//                 CgstAmount = grn.CGSTTotal ?? 0m,
//                 SgstAmount = grn.SGSTTotal ?? 0m,
//                 IgstAmount = grn.IGSTTotal ?? 0m,
//                 OtherCharges = grn.MiscCharges ?? 0m,
//                 RoundOff = grn.RoundOff ?? 0m,
//                 GrandTotal = grn.PurchaseValue ?? 0m,

//                 AttachmentPath = null,              // if any
//                 Remarks = grn.Remarks,
//                 IsBillAcccounted = false             // new bill → not accounted yet
//             };

//             // 🔸 4.3: Build Detail lines
//             foreach (var d in usedDetails)
//             {
//                 var line = new PurchaseBillEntryDetailDto
//                 {
//                     ItemId = d.ItemId,
//                     GrnDetailId = d.Id,
//                     PoDetailId = d.PoSlNoLocal ?? 0,
//                     PoQty = d.OrderQuantity,
//                     GrnQty = d.QcAcceptedQuantity ?? d.ReceivedQuantity,
//                     BilledQty = d.QcAcceptedQuantity ?? d.ReceivedQuantity,

//                     PoRate = d.UnitPrice ?? 0,
//                     BilledRate = d.UnitPrice ?? 0,
//                     UomId = d.UOMId,

//                     TaxPercentage = d.GSTPercentage ?? 0,
//                     DiscountAmount = d.DiscountValue ?? 0,

//                     // Base / taxable / taxes / line total
//                     LineBaseAmount = d.ItemValue
//                                      ?? ((d.QcAcceptedQuantity ?? d.ReceivedQuantity) * (d.UnitPrice ?? 0)),

//                     TaxableAmount = d.TaxableAmount
//                                     ?? (d.ItemValue ?? 0) - (d.DiscountValue ?? 0),

//                     CgstAmount = d.CGST ?? 0,
//                     SgstAmount = d.SGST ?? 0,
//                     IgstAmount = d.IGST ?? 0,
//                 };

//                 line.LineTotal = line.TaxableAmount + line.CgstAmount + line.SgstAmount + line.IgstAmount;

//                 headerDto.Lines.Add(line);
//             }

//             // 🔸 4.4: Call bill entry command handler
//             var cmd = new CreatePurchaseBillEntryCommand(headerDto);
//             var billId = await _mediator.Send(cmd, ct);

//             return billId;
//         }

//     }
// }