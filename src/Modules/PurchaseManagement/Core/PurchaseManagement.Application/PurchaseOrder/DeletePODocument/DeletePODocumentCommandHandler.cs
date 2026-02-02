// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.DeletePODocument
// {
//     public class DeletePODocumentCommandHandler
//         : IRequestHandler<DeletePODocumentCommand, ApiResponseDTO<bool>>
//     {
//         private readonly IFileUploadService _fileUploadService;
//         private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly ICompanyGrpcClient _companyGrpcClient;

//         public DeletePODocumentCommandHandler(
//             IFileUploadService fileUploadService,
//             IIPAddressService ipAddressService,
//             IPODocumentQueryRepository poDocumentQueryRepository,
//             IUnitGrpcClient unitGrpcClient,
//             ICompanyGrpcClient companyGrpcClient)
//         {
//             _fileUploadService = fileUploadService;
//             _ipAddressService = ipAddressService;
//             _poDocumentQueryRepository = poDocumentQueryRepository;
//             _unitGrpcClient = unitGrpcClient;
//             _companyGrpcClient = companyGrpcClient;
//         }

//         public async Task<ApiResponseDTO<bool>> Handle(DeletePODocumentCommand request, CancellationToken cancellationToken)
//         {
//             // 1️⃣ Resolve company & unit names via gRPC
//             var companyId = _ipAddressService.GetCompanyId();
//             var unitId    = _ipAddressService.GetUnitId();

//             var companies = await _companyGrpcClient.GetAllCompanyAsync();
//             var units     = await _unitGrpcClient.GetAllUnitAsync();

//             var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
//             var unitLookup    = units.ToDictionary(u => u.UnitId, u => u.UnitName);

//             var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
//             var unitName    = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;

//             // 2️⃣ Base directory from DB/config
//             var baseDirectory = await _poDocumentQueryRepository.GetDocumentDirectoryAsync();
//             if (string.IsNullOrWhiteSpace(baseDirectory))
//             {
//                 return new ApiResponseDTO<bool>
//                 {
//                     IsSuccess = false,
//                     Message   = "Base directory not configured.",
//                     Data      = false
//                 };
//             }

//             // 3️⃣ Build physical path
//             var uploadPath = Path.Combine(
//                 Directory.GetCurrentDirectory(),
//                 "Resources",
//                 baseDirectory,
//                 companyName ?? string.Empty,
//                 unitName ?? string.Empty);

//             var filePath = Path.Combine(uploadPath, request.PODocumentPath ?? string.Empty);

//             if (!File.Exists(filePath))
//             {
//                 return new ApiResponseDTO<bool>
//                 {
//                     IsSuccess = false,
//                     Message   = "The specified file does not exist.",
//                     Data      = false
//                 };
//             }

//             // 4️⃣ Delete physical file
//             var result = await _fileUploadService.DeleteFileAsync(filePath);
//             if (!result)
//             {
//                 return new ApiResponseDTO<bool>
//                 {
//                     IsSuccess = false,
//                     Message   = "File deletion failed.",
//                     Data      = false
//                 };
//             }

//             // 5️⃣ Delete DB metadata if IDs are provided
//             if (request.Id > 0 && request.POId > 0)
//             {
//                 var dbResult = await _poDocumentQueryRepository.DeleteFileDetailsDocumentAsync(
//                     request.Id,
//                     request.POId,
//                     request.FileName ?? string.Empty);

//                 if (!dbResult)
//                 {
//                     return new ApiResponseDTO<bool>
//                     {
//                         IsSuccess = false,
//                         Message   = "Document entry not found or deletion failed in DB.",
//                         Data      = false
//                     };
//                 }
//             }

//             // 6️⃣ Success
//             return new ApiResponseDTO<bool>
//             {
//                 IsSuccess = true,
//                 Message   = "PO document deleted successfully.",
//                 Data      = true
//             };
//         }
//     }
// }
