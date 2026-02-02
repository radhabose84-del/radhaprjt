// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Common;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove
// {
//     public class GetSesApprovalListHandler : IRequestHandler<GetSesApprovalListQuery, List<SesApprovalListDto>>
//     {
//         private readonly IServicePurchaseOrderQueryRepository _servicePoQuery;
//         private readonly IMiscMasterQueryRepository _misc;
//         private readonly IIPAddressService _ip;
//         private readonly IMapper _mapper;

//         public GetSesApprovalListHandler( IServicePurchaseOrderQueryRepository servicePoQuery, IMiscMasterQueryRepository misc, IIPAddressService ip, IMapper mapper)
//         {
//             _servicePoQuery = servicePoQuery;
//             _misc = misc;
//             _ip = ip;
//             _mapper = mapper;
//         }
//         public async Task<List<SesApprovalListDto>> Handle(GetSesApprovalListQuery request, CancellationToken ct)
//         {
//             // 🔹 1. Resolve "Pending" approval status from Misc
//             var pendingStatus = await _misc.GetMiscMasterByName( MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

//             var unitId = _ip.GetUnitId();

//             // 🔹 2. Fetch SES rows for approval
//             var list = await _servicePoQuery.GetServiceEntrySheetsForApprovalAsync(
               
              
//                 fromDate: request.FromDate,
//                 toDate: request.ToDate,
//                 vendorId: request.VendorId,
//                 ct: ct);

//             // 🔹 3. Map to DTO
//             return _mapper.Map<List<SesApprovalListDto>>(list);
//         }
//     }
// }