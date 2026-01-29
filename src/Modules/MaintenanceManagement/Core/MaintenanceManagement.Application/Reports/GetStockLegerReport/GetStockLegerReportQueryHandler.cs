// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IReports;
// using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
// using MaintenanceManagement.Application.Reports.GetStockLegerReport;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.StockLedger.Queries.GetStockLegerReport
// {
//     public class GetStockLegerReportQueryHandler : IRequestHandler<GetStockLegerReportQuery,ApiResponseDTO<List<StockLedgerReportDto>>>
//     {
//         private readonly IReportRepository _stockLedgerQueryRepository;        
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient; // 👈 gRPC Inject here

//         public GetStockLegerReportQueryHandler(IReportRepository stockLedgerQueryRepository, IMapper mapper, IMediator mediator, IDepartmentAllGrpcClient departmentAllGrpcClient)
//         {
//             _stockLedgerQueryRepository = stockLedgerQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<StockLedgerReportDto>>> Handle(GetStockLegerReportQuery request, CancellationToken cancellationToken)
//         {
//             var result = await _stockLedgerQueryRepository.GetSubStoresStockLedger(request.OldUnitcode, request.FromDate, request.ToDate, request.ItemCode,request.DepartmentId);
//             var substores = _mapper.Map<List<StockLedgerReportDto>>(result);
//               // 🔥 Fetch departments using gRPC
//             var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

//             foreach (var data in substores)
//             {
//                 if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
//                 {
//                     data.DepartmentName = departmentName;
//                 }
//             }
//              //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                     actionDetail: "SubStoresStockLedgerReport",
//                     actionCode: "GetStockLegerReportQuery",        
//                     actionName: substores.Count.ToString(),
//                     details: $"Stock details was fetched.",
//                     module:"SubStoresStockLedgerReport"
//                 );
//                 await _mediator.Publish(domainEvent, cancellationToken);
//             return new ApiResponseDTO<List<StockLedgerReportDto>> { IsSuccess = true, Message = "Success", Data = substores };
//         }
//     }
// }