// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.Interfaces.IMRS;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.MRS.Queries.GetMrsEntryById
// {
//     public class GetMrsEntryByIdQueryHandler : IRequestHandler<GetMrsEntryByIdQuery, GetMrsEntryByIdDto>
//     {
//         private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUOMGrpcClient _uOMGrpcClient;
//         private readonly IInventoryGrpcClient _inventoryGrpcClient;

//         public GetMrsEntryByIdQueryHandler(IMrsEntryQueryRepository iMrsEntryQueryRepository, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient, IUOMGrpcClient uOMGrpcClient, IInventoryGrpcClient inventoryGrpcClient)
//         {
//             _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//             _inventoryGrpcClient = inventoryGrpcClient;
//         }

//         public async Task<GetMrsEntryByIdDto> Handle(GetMrsEntryByIdQuery request, CancellationToken cancellationToken)
//         {
//             // 1️⃣ Fetch Header + Details
//             var dto = await _iMrsEntryQueryRepository.GetMrsDetailsById(request.Id);
//             if (dto == null)
//                 throw new KeyNotFoundException("Mrs not found");

//             // 2️⃣ Collect IDs
//             var departmentIds = new List<int> { dto.DepartmentId, dto.SubDepartmentId }.Distinct().ToList();
//             var uomIds = dto.MrsDetails.Select(x => x.UomId).Distinct().ToList();
//             var itemIds = dto.MrsDetails.Select(x => x.ItemId).Distinct().ToList();

//             // 3️⃣ Fire parallel gRPC calls
//             var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var uomTasks = uomIds.Select(id => _uOMGrpcClient.GetByIdAsync(id, cancellationToken)).ToList();
//             var itemTask = _inventoryGrpcClient.GetItemPurchaseToleranceAsync(itemIds, cancellationToken);

//             // 4️⃣ Await all together
//             await Task.WhenAll(
//                 departmentTask,
//                 Task.WhenAll(uomTasks),
//                 itemTask
//             );

//             var departments = await departmentTask;
//             var uoms = uomTasks.Select(t => t.Result).Where(u => u != null).ToDictionary(u => u.Id, u => u);
//             var items = await itemTask;

//             // 5️⃣ Prepare lookup dictionaries
//             var departmentById = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var itemById = items.ToDictionary(i => i.ItemId, i => i);

//             // 6️⃣ Enrich Header
//             dto.DepartmentName = departmentById.GetValueOrDefault(dto.DepartmentId, "NA");
//             dto.SubDepartmentName = departmentById.GetValueOrDefault(dto.SubDepartmentId, "NA");

//             // 7️⃣ Enrich Details
//             foreach (var line in dto.MrsDetails)
//             {
//                 // 🧩 Item Info
//                 if (itemById.TryGetValue(line.ItemId, out var item))
//                 {
//                     line.ItemCode = item.ItemCode ?? "NA";
//                     line.ItemName = item.ItemName ?? "NA";
//                 }
//                 else
//                 {
//                     line.ItemCode = "NA";
//                     line.ItemName = "NA";
//                 }

//                 // 📏 UOM Info
//                 if (uoms.TryGetValue(line.UomId, out var uom))
//                 {
//                     line.UomName = uom.UOMName ?? "NA";
//                 }
//                 else
//                 {
//                     line.UomName = "NA";
//                 }
//             }

//             // 8️⃣ Audit log
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "GetMrsEntryByIdQuery",
//                 actionName: dto.Id.ToString(),
//                 details: $"Mrs details {dto.Id} fetched.",
//                 module: "MrsEntry"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);
//             return dto;
//         }
//     }
// }