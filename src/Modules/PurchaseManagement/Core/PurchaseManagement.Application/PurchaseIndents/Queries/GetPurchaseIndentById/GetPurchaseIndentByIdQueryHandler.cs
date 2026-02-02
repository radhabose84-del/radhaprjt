// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Inventory;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById
// {
//     public class GetPurchaseIndentByIdQueryHandler : IRequestHandler<GetPurchaseIndentByIdQuery, IndentByIdDto>
//     {
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IMediator _mediator;
//         private readonly IMapper _mapper;
//         private readonly IItemGrpcClient _itemGrpcClient;
//         private readonly IUOMGrpcClient _uOMGrpcClient;
//         private readonly IInventoryCategoryGrpcClient _inventoryCategoryGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;        

//         public GetPurchaseIndentByIdQueryHandler(
//             IPurchaseIndentQuery purchaseIndentQuery,
//             IMediator mediator,
//             IMapper mapper,
//             IItemGrpcClient itemGrpcClient,
//             IUOMGrpcClient uOMGrpcClient,
//             IInventoryCategoryGrpcClient inventoryCategoryGrpcClient,
//             IDepartmentAllGrpcClient departmentAllGrpcClient,
//             IUnitGrpcClient unitGrpcClient)
//         {
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _mediator = mediator;
//             _mapper = mapper;
//             _itemGrpcClient = itemGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//             _inventoryCategoryGrpcClient = inventoryCategoryGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//         }

//         public async Task<IndentByIdDto> Handle(GetPurchaseIndentByIdQuery request, CancellationToken cancellationToken)
//         {
//             var result = await _purchaseIndentQuery.GetByIdAsync(request.Id);
//             var indent = _mapper.Map<IndentByIdDto>(result);

//             indent.IndentDetails ??= new List<IndentDetailByIdDto>();

//             // If no details, still log audit & return
//             if (indent.IndentDetails.Count == 0)
//             {
//                 await PublishAuditAsync(request, cancellationToken);
//                 return indent;
//             }

//             // 1) Collect ids
//             var itemIds = indent.IndentDetails.Select(d => d.ItemId).Distinct().ToList();
//             var itemCategoryIds = indent.IndentDetails.Select(d => d.ItemCategoryId).Distinct().ToList();

//             // 2) Get items (needed for SourceId filtering + IsOnSpot)
//             var itemdata = await _itemGrpcClient.GetItemsByIdsAsync(itemIds, cancellationToken)
//                           ?? new List<ItemMasterDto>();

//             // 3) If SourceId filter passed -> filter indent details by ItemMasterDto.SourceId
//             var sourceId = request.SourceId.GetValueOrDefault();
//             if (sourceId > 0)
//             {
//                 var allowedItemIds = itemdata
//                     .Where(x => x.SourceOfItem == sourceId)
//                     .Select(x => x.Id)
//                     .ToHashSet();

//                 indent.IndentDetails = indent.IndentDetails
//                     .Where(d => allowedItemIds.Contains(d.ItemId))
//                     .ToList();

//                 // rebuild after filter
//                 itemIds = indent.IndentDetails.Select(d => d.ItemId).Distinct().ToList();
//                 itemCategoryIds = indent.IndentDetails.Select(d => d.ItemCategoryId).Distinct().ToList();

//                 // also shrink itemdata to match
//                 itemdata = itemdata.Where(x => allowedItemIds.Contains(x.Id)).ToList();
//             }

//             // If after filtering there are no lines, return header with empty detail list
//             if (indent.IndentDetails.Count == 0)
//             {
//                 // still enrich header names
//                 var departmentDataEmpty = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//                 var unitDataEmpty = await _unitGrpcClient.GetAllUnitAsync();

//                 var deptLookupEmpty = departmentDataEmpty.GroupBy(d => d.DepartmentId)
//                     .ToDictionary(g => g.Key, g => g.First().DepartmentName);

//                 var unitLookupEmpty = unitDataEmpty.GroupBy(d => d.UnitId)
//                     .ToDictionary(g => g.Key, g => g.First().UnitName);

//                 if (deptLookupEmpty.TryGetValue(indent.DepartmentId, out var deptName))
//                     indent.DepartmentName = deptName;

//                 if (unitLookupEmpty.TryGetValue(indent.UnitId, out var unitName))
//                     indent.UnitName = unitName;

//                 await PublishAuditAsync(request, cancellationToken);
//                 return indent;
//             }

//             // 4) Other lookups
//             var uomData = await _uOMGrpcClient.GetUOMAsync();
//             var categoryData = await _inventoryCategoryGrpcClient.GetCategoryByIdsAsync(itemCategoryIds, cancellationToken);
//             var departmentData = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var unitData = await _unitGrpcClient.GetAllUnitAsync();

//             // 5) Dictionaries (safe for duplicates)
//             var itemLookup = itemdata
//                 .GroupBy(d => d.Id)
//                 .ToDictionary(g => g.Key, g => g.First()); 

//             var uomLookup = uomData
//                 .GroupBy(d => d.Id)
//                 .ToDictionary(g => g.Key, g => g.First().UOMName);

//             var categoryLookup = categoryData
//                 .GroupBy(d => d.Id)
//                 .ToDictionary(g => g.Key, g => g.First().ItemCategoryName);

//             var departmentLookup = departmentData
//                 .GroupBy(d => d.DepartmentId)
//                 .ToDictionary(g => g.Key, g => g.First().DepartmentName);

//             var unitLookup = unitData
//                 .GroupBy(d => d.UnitId)
//                 .ToDictionary(g => g.Key, g => g.First().UnitName);

//             // 6) Header enrichment
//             if (departmentLookup.TryGetValue(indent.DepartmentId, out var deptName2))
//                 indent.DepartmentName = deptName2;

//             if (unitLookup.TryGetValue(indent.UnitId, out var unitName2))
//                 indent.UnitName = unitName2;

//             // 7) Line enrichment (ItemName + SourceId + IsOnSpot + GST + HSN)
//             foreach (var dto in indent.IndentDetails)
//             {
//                 if (itemLookup.TryGetValue(dto.ItemId, out var item))
//                 {
//                     dto.ItemName = item.ItemName;
//                     dto.SourceId = item.SourceOfItem;
//                     dto.IsOnSpot = item.IsOnSpot;
//                     dto.GSTPercentage = item.GSTPercentage;
//                     dto.HSNCode = item.HSNCode;
                    
//                     if (item.Vendors != null && item.Vendors.Count > 0)
//                     {
//                         // To avoid sharing same list instance, clone it
//                         dto.Vendors = item.Vendors
//                             .Select(v => new ItemVendorDto
//                             {
//                                 SupplierId = v.SupplierId,
//                                 UnitId = v.UnitId,
//                                 SupplierPartNo = v.SupplierPartNo,
//                                 DefaultSupplier = v.DefaultSupplier,
//                                 LeadTime = v.LeadTime,
//                                 MOQ = v.MOQ,
//                                 MOQUomId = v.MOQUomId,
//                                 PackageUomId = v.PackageUomId,
//                                 PackageValue = v.PackageValue
//                             })
//                             .ToList();
//                     }
//                     else
//                     {
//                         dto.Vendors = null;
//                     }
//                 }

//                 if (uomLookup.TryGetValue(dto.ItemUOMId, out var uomName))
//                     dto.ItemUOM = uomName;

//                 if (categoryLookup.TryGetValue(dto.ItemCategoryId, out var catName))
//                     dto.ItemCategory = catName;
//             }

//             await PublishAuditAsync(request, cancellationToken);
//             return indent;
//         }

//         private async Task PublishAuditAsync(GetPurchaseIndentByIdQuery request, CancellationToken ct)
//         {
//             var evt = new AuditLogsDomainEvent(
//                 actionDetail: "GetIndentById",
//                 actionCode: "GetIndentById",
//                 actionName: "GetIndentById",
//                 details: JsonSerializer.Serialize(request),
//                 module: "PurchaseIndent"
//             );

//             await _mediator.Publish(evt, ct);
//         }
//     }
// }
