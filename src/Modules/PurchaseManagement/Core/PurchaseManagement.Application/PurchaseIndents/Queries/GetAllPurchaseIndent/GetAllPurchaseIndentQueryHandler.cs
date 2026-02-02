// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent
// {
//     public class GetAllPurchaseIndentQueryHandler : IRequestHandler<GetAllPurchaseIndentQuery, ApiResponseDTO<List<Core.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>>>
//     {
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IMediator _mediator;
//         private readonly IMapper _mapper;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         public GetAllPurchaseIndentQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMediator mediator, IMapper mapper, IUnitGrpcClient unitGrpcClient,
//             IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _mediator = mediator;
//             _mapper = mapper;
//             _unitGrpcClient = unitGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }
//         public async Task<ApiResponseDTO<List<Core.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>>> Handle(GetAllPurchaseIndentQuery request, CancellationToken cancellationToken)
//         {
//             var (indents, totalCount) =
//             await _purchaseIndentQuery.GetAllPurchaseIndentAsync(
//                 request.PageNumber,
//                 request.PageSize,
//                 request.SearchTerm,
//                 request.StatusId
//             );

//         var units = await _unitGrpcClient.GetAllUnitAsync();
//         var unitLookup = units.ToDictionary(x => x.UnitId, x => x.UnitName);

//         var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//         var deptLookup = departments.ToDictionary(x => x.DepartmentId, x => x.DepartmentName);

//         foreach (var indent in indents)
//         {
//             if (unitLookup.TryGetValue(indent.UnitId, out var unitName))
//                 indent.UnitName = unitName;

//             if (deptLookup.TryGetValue(indent.DepartmentId, out var deptName))
//                 indent.DepartmentName = deptName;
//         }

//         await _mediator.Publish(
//             new AuditLogsDomainEvent(
//                 "GetAll",
//                 "GetAll",
//                 "GetAll",
//                 JsonSerializer.Serialize(request),
//                 "PurchaseIndent"
//             ),
//             cancellationToken
//         );

//         return new ApiResponseDTO<List<IndentDto>>
//         {
//             IsSuccess = true,
//             Message = "Success",
//             Data = indents,
//             TotalCount = totalCount,
//             PageNumber = request.PageNumber,
//             PageSize = request.PageSize
//         };
           
//         }
//     }
// }