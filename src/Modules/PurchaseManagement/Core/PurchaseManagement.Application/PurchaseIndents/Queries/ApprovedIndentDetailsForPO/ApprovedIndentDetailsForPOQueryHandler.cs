// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
// using PurchaseManagement.Application.DutyMaster; // DutyMasterReadDto
// using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
// {
//     public class ApprovedIndentDetailsForPOQueryHandler
//         : IRequestHandler<ApprovedIndentDetailsForPOQuery, List<IndentForPODto>>
//     {
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IMediator _mediator;
//         private readonly IMapper _mapper;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IItemGrpcClient _itemGrpcClient;
//         private readonly IUOMGrpcClient _uOMGrpcClient;
//         private readonly IInventoryCategoryGrpcClient _inventoryCategoryGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IPriceMasterQueryRepository _priceMasterQueryRepository;
//         private readonly IPurchaseOrderQueryRepository _purchaseOrderQueryRepository;
//         private readonly IDutyMasterQueryRepository _dutyRepo;

//         public ApprovedIndentDetailsForPOQueryHandler(
//             IPurchaseIndentQuery purchaseIndentQuery,
//             IMediator mediator,
//             IMapper mapper,
//             IWorkflowGrpcClient workflowGrpcClient,
//             IUsersAllGrpcClient usersAllGrpcClient,
//             IIPAddressService ipAddressService,
//             IItemGrpcClient itemGrpcClient,
//             IUOMGrpcClient uOMGrpcClient,
//             IInventoryCategoryGrpcClient inventoryCategoryGrpcClient,
//             IDepartmentAllGrpcClient departmentAllGrpcClient,
//             IUnitGrpcClient unitGrpcClient,
//             IPriceMasterQueryRepository priceMasterQueryRepository,
//             IPurchaseOrderQueryRepository purchaseOrderQueryRepository,
//             IDutyMasterQueryRepository dutyRepo)
//         {
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _mediator = mediator;
//             _mapper = mapper;
//             _workflowGrpcClient = workflowGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;
//             _ipAddressService = ipAddressService;
//             _itemGrpcClient = itemGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//             _inventoryCategoryGrpcClient = inventoryCategoryGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//             _priceMasterQueryRepository = priceMasterQueryRepository;
//             _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
//             _dutyRepo = dutyRepo;
//         }

//         public async Task<List<IndentForPODto>> Handle(ApprovedIndentDetailsForPOQuery request, CancellationToken cancellationToken)
//         {
//             // 1) Base data from DB
//             // var raw = await _purchaseIndentQuery.GetApprovedIndentDetailsForPO(request.VendorId, cancellationToken);
//             //var indents = _mapper.Map<List<IndentForPODto>>(raw) ?? new List<IndentForPODto>();
//             var indents = await _purchaseIndentQuery.GetApprovedIndentDetailsForPO(request.VendorId,request.DepartmentId,cancellationToken)
//              ?? new List<IndentForPODto>();

//             var approved = new List<IndentForPODto>();

//             foreach (var header in indents)
//             {
//                 // Ensure non-null collections
//                 header.IndentDetails ??= new List<IndentDetailsForPODto>();
//                 header.IndentDutyDetails ??= new List<IndentDutyForPODto>();

//                 if (header.IndentDetails.Count == 0)
//                 {
//                     approved.Add(header);
//                     continue;
//                 }

//                 // 2) Collect ids
//                 var itemIds = header.IndentDetails.Select(d => d.ItemId).Distinct().ToList();
//                 var itemCategoryIds = header.IndentDetails.Select(d => d.ItemCategoryId).Distinct().ToList();

//                 // 3) External lookups
//                 var itemdata     = await _itemGrpcClient.GetItemsByIdsAsync(itemIds, cancellationToken);
//                 var uomData      = await _uOMGrpcClient.GetUOMAsync();
//                 var categoryData = await _inventoryCategoryGrpcClient.GetCategoryByIdsAsync(itemCategoryIds, cancellationToken);
//                 var deptData     = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//                 var unitData     = await _unitGrpcClient.GetAllUnitAsync();

//                 // 4) Dictionaries (handle duplicates by key safely if needed)
//                 //var itemLookup        = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().ItemName);
//                 var itemLookup = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First());  
//                 var uomLookup         = uomData.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().UOMName);
//                 var categoryLookup    = categoryData.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().ItemCategoryName);
//                 var departmentLookup  = deptData.GroupBy(d => d.DepartmentId).ToDictionary(g => g.Key, g => g.First().DepartmentName);
//                 var unitLookup        = unitData.GroupBy(d => d.UnitId).ToDictionary(g => g.Key, g => g.First().UnitName);

//                 var itemTariffLookup  = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().TariffNumber ?? string.Empty);
//                 var itemHsnLookup     = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().HSNCode      ?? string.Empty);
//                 var itemGSTLookup     = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().GSTPercentage);
//                 var hsnLookup         = itemdata.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First().HSNCode ?? string.Empty);

//                 // 5) Prices
//                 var itemIdQtys = header.IndentDetails
//                     .Select(d => new ItemQtyDto { ItemId = d.ItemId, Qty = d.QuantityRequired })
//                     .ToList();

//                // var unitPriceData = await _priceMasterQueryRepository.GetUnitPriceByQtyANDItemId(itemIdQtys);
//                 //var unitPriceLookup = unitPriceData.GroupBy(d => d.ItemId).ToDictionary(g => g.Key, g => g.First().UnitPrice);

//                 var lastPOPrice = await _purchaseOrderQueryRepository.LastPOPriceByItemIdAsync(itemIds);
//                 var lastPOPriceLookup = lastPOPrice.GroupBy(d => d.ItemId).ToDictionary(g => g.Key, g => g.First().LastPOPrice);

//                 // 6) Duty query (by Tariff/HSN + effective date)
//                 var headerTariffs = header.IndentDetails
//                     .Select(x => itemTariffLookup.TryGetValue(x.ItemId, out var t) ? t : null)
//                     .Where(s => !string.IsNullOrWhiteSpace(s))
//                     .Distinct(StringComparer.OrdinalIgnoreCase)
//                     .ToArray();

//                 var headerHsns = header.IndentDetails
//                     .Select(x => itemHsnLookup.TryGetValue(x.ItemId, out var h) ? h : null)
//                     .Where(s => !string.IsNullOrWhiteSpace(s))
//                     .Distinct(StringComparer.OrdinalIgnoreCase)
//                     .ToArray();

//                 var indentDate = new DateTimeOffset(header.IndentDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
//                 var dutyRows = await _dutyRepo.GetByTariffOrHsnAsync(headerTariffs, headerHsns, indentDate, cancellationToken);

//                 // 7) Header-level enrichments
//                 if (departmentLookup.TryGetValue(header.DepartmentId, out var deptName))
//                     header.DepartmentName = deptName;

//                 if (unitLookup.TryGetValue(header.UnitId, out var unitName))
//                     header.UnitName = unitName;

//                 // 8) Line-level enrichments
//                 foreach (var dto in header.IndentDetails)
//                 {
//                     if (itemLookup.TryGetValue(dto.ItemId, out var item))
//                         dto.ItemName = item.ItemName;
//                         dto.IsOnSpot = item.IsOnSpot; 

//                     if (uomLookup.TryGetValue(dto.ItemUOMId, out var uomName))
//                         dto.UOMName = uomName;

//                     if (categoryLookup.TryGetValue(dto.ItemCategoryId, out var categoryName))
//                         dto.ItemCategoryName = categoryName;

//                     if (itemGSTLookup.TryGetValue(dto.ItemId, out var gst))
//                         dto.GSTPercentage = gst;

//                     if (hsnLookup.TryGetValue(dto.ItemId, out var hsn))
//                         dto.HSNCode = hsn;

//                  //   if (unitPriceLookup.TryGetValue(dto.ItemId, out var unitPrice))
//                      //   dto.UnitPrice = unitPrice;

//                     if (lastPOPriceLookup.TryGetValue(dto.ItemId, out var lastPrice))
//                         dto.LastPOPrice = lastPrice;
//                 }

//                 // 9) Build distinct header-duty list from matching items
//                 var dutiesForHeader = new List<DutyMasterReadDto>();

//                 foreach (var line in header.IndentDetails)
//                 {
//                     itemTariffLookup.TryGetValue(line.ItemId, out var tno);
//                     itemHsnLookup.TryGetValue(line.ItemId, out var hsn);

//                     var pick =
//                         dutyRows.FirstOrDefault(d => stringsEq(d.TariffNumber, tno) && stringsEq(d.HsnCode, hsn))
//                         ?? dutyRows.FirstOrDefault(d => stringsEq(d.TariffNumber, tno))
//                         ?? dutyRows.FirstOrDefault(d => stringsEq(d.HsnCode, hsn));

//                     if (pick is not null)
//                         dutiesForHeader.Add(pick);
//                 }

//                 dutiesForHeader = dutiesForHeader
//                     .GroupBy(d => d.Id)
//                     .Select(g => g.First())
//                     .ToList();

//                 header.IndentDutyDetails = _mapper.Map<List<IndentDutyForPODto>>(dutiesForHeader);

//                 approved.Add(header);
//             }

//             // 10) Audit log
//             var evt = new AuditLogsDomainEvent(
//                 actionDetail: "ApprovedIndentDetailsForPOQuery",
//                 actionCode:   "ApprovedIndentDetailsForPOQuery",
//                 actionName:   "ApprovedIndentDetailsForPOQuery",
//                 details:      JsonSerializer.Serialize(request),
//                 module:       "PurchaseIndent"
//             );
//             await _mediator.Publish(evt, cancellationToken);

//             // Return the enriched list you built
//             return approved;
//         }

//         private static bool stringsEq(string? a, string? b) =>
//             string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.OrdinalIgnoreCase);
//     }
// }
