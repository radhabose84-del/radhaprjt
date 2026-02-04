// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Dtos.Users;
// using Contracts.Interfaces.External.IUser;
// using MediatR;

// namespace InventoryManagement.Application.Reports.GetUnitsByDivision
// {
//     public class GetUnitsByDivisionQueryHandler : IRequestHandler<GetUnitsByDivisionQuery, List<DivisionUnitDto>>
//     {
//         private readonly IDivisionUnitGrpcClient _divisionUnitGrpcClient;
//         public GetUnitsByDivisionQueryHandler(IDivisionUnitGrpcClient divisionUnitGrpcClient)
//         {
//             _divisionUnitGrpcClient = divisionUnitGrpcClient;
//         }
//           public async Task<List<DivisionUnitDto>> Handle(
//             GetUnitsByDivisionQuery request,
//             CancellationToken cancellationToken)
//         {
//             return await _divisionUnitGrpcClient.GetUnitsByDivisionAsync(
//                 request.CompanyId,
//                 request.DivisionId,
//                 cancellationToken);
//         }

//     }
// }