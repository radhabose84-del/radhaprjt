// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.Quotation.QuotationEntry.Commands.DeleteImage
// {
//     public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, ApiResponseDTO<bool>>
//     {        
//         private readonly IQuotationCommandRepository _quotationCommandRepository;
//         private readonly ILogger<DeleteFileCommandHandler> _logger;
//         private readonly IFileUploadService _fileUploadService;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly ICompanyGrpcClient _companyGrpcClient;

//         public DeleteFileCommandHandler(ILogger<DeleteFileCommandHandler> logger, IQuotationCommandRepository quotationCommandRepository, IFileUploadService fileUploadService, IIPAddressService ipAddressService,  IUnitGrpcClient unitGrpcClient,
//             ICompanyGrpcClient companyGrpcClient)
//         {
//             _logger = logger;
//             _quotationCommandRepository = quotationCommandRepository;
//             _fileUploadService = fileUploadService;
//             _ipAddressService = ipAddressService;
//             _unitGrpcClient = unitGrpcClient;
//             _companyGrpcClient = companyGrpcClient;
//         }

//         public async Task<ApiResponseDTO<bool>>  Handle(DeleteFileCommand request, CancellationToken cancellationToken)
//         { 
//             var companyId = _ipAddressService.GetCompanyId();
//             var unitId = _ipAddressService.GetUnitId();
//             var companies = await _companyGrpcClient.GetAllCompanyAsync();
//             var units = await _unitGrpcClient.GetAllUnitAsync();

//             var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
//             var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

//             var companyName = companyLookup.TryGetValue(companyId, out var cname) ? cname : string.Empty;
//             var unitName = unitLookup.TryGetValue(unitId, out var uname) ? uname : string.Empty;   

//             string baseDirectory = await _quotationCommandRepository.GetBaseDirectoryAsync();
//             if (string.IsNullOrWhiteSpace(baseDirectory))
//             {
//                 _logger.LogError("Base directory path not found in database.");
//                 throw new Exception("Base directory not configured.");                            
//             }                        
//             string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);         

//             string filePath = Path.Combine(uploadPath, request.imagePath??string.Empty);

//             var result = await _fileUploadService.DeleteFileAsync(filePath);

//             await _quotationCommandRepository.RemoveImageReferenceAsync(request.imagePath);

//             if (result)
//             {
//                 return new ApiResponseDTO<bool> { IsSuccess = true, Message = "File deleted successfully" };                
//             }            
//             return new ApiResponseDTO<bool> { IsSuccess = false, Message = "File deletion failed" };          
//         }
//     }
// }
