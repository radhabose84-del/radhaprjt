// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO
// {
//     public class GetServicePOByIdQueryHandler : IRequestHandler<GetServicePOByIdQuery, PurchaseOrderServiceDetailDto>
//     {

//         private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
//         private readonly IMapper _mapper;

//         private readonly ICurrencyGrpcClient _currencyGrpcClient;
//         private readonly IUOMGrpcClient _uOMGrpcClient;

//         public GetServicePOByIdQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, ICurrencyGrpcClient currencyGrpcClient, IUOMGrpcClient uOMGrpcClient)
//         {
//             _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
//             _mapper = mapper;
//             _currencyGrpcClient = currencyGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//         }
        
//         public async Task<PurchaseOrderServiceDetailDto?> Handle(GetServicePOByIdQuery request, CancellationToken ct)
//         {
//             // 1) Fetch stitched PO (header -> service headers -> lines -> schedules, payment terms)
//             var entity = await _servicePurchaseOrderQueryRepository.GetServicePOByIdAsync(request.Id, ct);
//             if (entity is null) return null;

//             // 2) Map to UI DTO
//             var dto = _mapper.Map<PurchaseOrderServiceDetailDto>(entity);
//             if (dto is null) return null;

//             // 3) Enrich UOM on service lines via gRPC (best-effort, non-fatal)
//             try
//             {
//                 var lines = dto.ServicePo?
//                     .SelectMany(h => h.Lines ?? Enumerable.Empty<PurchaseOrderServiceLineDto>())
//                     .ToList() ?? new List<PurchaseOrderServiceLineDto>();

//                 var neededIds = lines
//                     .Where(l => l.UOMId.HasValue && l.UOMId.Value > 0)
//                     .Select(l => l.UOMId!.Value)
//                     .Distinct()
//                     .ToArray();

//                 if (neededIds.Length > 0)
//                 {
//                     var uoms = await _uOMGrpcClient.GetUOMAsync(); // add ct if your client supports it
//                     if (uoms is not null && uoms.Count > 0)
//                     {
//                         // Prefer Code -> UOMName/Name -> Id as fallback
//                         static string PickLabel(dynamic u) =>
//                             !string.IsNullOrWhiteSpace(u.Code) ? u.Code
//                             : (!string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName
//                             : (!string.IsNullOrWhiteSpace(u.Name) ? u.Name
//                             : u.Id.ToString()));

//                         var uomLookup = uoms
//                             .Where(u => neededIds.Contains(u.Id))
//                             .GroupBy(u => u.Id)
//                             .ToDictionary(g => g.Key, g => PickLabel(g.First()));

//                         foreach (var line in lines)
//                         {
//                             if (line.UOMId is int id && uomLookup.TryGetValue(id, out var label))
//                                 line.UOM = label; // maps to your PurchaseOrderServiceLineDto.UOM
//                         }
//                     }
//                 }
//             }
//             catch
//             {
//                 // swallow enrichment failures; return base dto
//             }

//             return dto;
//         }

//         //  public async Task<CreateServicePurchaseOrderDto?> Handle(GetServicePOByIdQuery request, CancellationToken ct)
//         // {


//         //     // repo returns stitched PurchaseOrderHeader with Service headers -> lines -> schedules, PaymentTerms
//         //     var entity = await _servicePurchaseOrderQueryRepository.GetServicePOByIdAsync(request.Id, ct);
//         //     if (entity is null) return null; // not found or not a Service PO

//         //     // map to the DTO you want on the UI (CreateServicePurchaseOrderDto)
//         //     var dto = _mapper.Map<CreateServicePurchaseOrderDto>(entity);


//         //     return dto;
//         // }
//     }
// }