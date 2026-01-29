// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IFixedAssetManagement;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
// using MediatR;

// namespace MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById
// {
//     public class GetAssetSpecificationByIdQueryHandler : IRequestHandler<GetAssetSpecificationByIdQuery, ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>>
//     {
//         private readonly IAssetSpecificationGrpcClient _assetSpecificationGrpcClient;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;

//         public GetAssetSpecificationByIdQueryHandler(IMapper mapper, IMediator mediator, IAssetSpecificationGrpcClient assetSpecificationGrpcClient)
//         {
//             _mapper = mapper;
//             _mediator = mediator;
//             _assetSpecificationGrpcClient = assetSpecificationGrpcClient;
//         }

//     public async Task<ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>> Handle(GetAssetSpecificationByIdQuery request, CancellationToken cancellationToken)
//        {
//             // 🔥 Fetch all specifications via gRPC
//             var assetSpecifications = await _assetSpecificationGrpcClient.GetAllAssetSpecificationAsync();

//             // ✅ Filter only the specifications for the requested AssetId
//             var filteredSpecifications = assetSpecifications
//                 .Where(d => d.AssetId == request.AssetId)
//                 .Select(d => new AssetSpecificationByAssetIdDto
//                 {
//                     AssetId = d.AssetId,
//                     SpecificationName = d.SpecificationName,
//                     SpecificationValue = d.SpecificationValue,
//                     CapitalizationDate = d.CapitalizationDate
//                 }).ToList();

//             // ✅ Return wrapped response
//             return new ApiResponseDTO<List<AssetSpecificationByAssetIdDto>>
//             {
//                 IsSuccess = true,
//                 Message = filteredSpecifications.Any() ? "Specifications found." : "No specifications found for the asset.",
//                 Data = filteredSpecifications,
//                 TotalCount = filteredSpecifications.Count         
//             };
//       }
//     }
// }