using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IExcelImport;
using MediatR;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class ImportAssetCommandHandler : IRequestHandler<ImportAssetCommand, ApiResponseDTO<bool>>
    {
        private readonly IExcelImportCommandRepository _assetRepository;
        private readonly IExcelImportQueryRepository _assetQueryRepository;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public ImportAssetCommandHandler(IExcelImportCommandRepository assetRepository, IExcelImportQueryRepository assetQueryRepository, IMapper mapper, IIPAddressService ipAddressService)
        {
            _assetRepository = assetRepository;
            _assetQueryRepository = assetQueryRepository;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task< ApiResponseDTO<bool>> Handle(ImportAssetCommand request, CancellationToken cancellationToken)
        {
            if (request.ImportDto == null || request.ImportDto.File == null || request.ImportDto.File.Length == 0)
            {
                //throw new ArgumentException("Invalid file uploaded.");
                  return new ApiResponseDTO<bool>
                            {
                                IsSuccess = false,
                                Message = "Invalid file uploaded",
                                Data = false
                            };
            }

            var assetsDto = new List<AssetMasterDto>();
            int currentRow = 0;

            using (var stream = new MemoryStream())
            {
                await request.ImportDto.File.CopyToAsync(stream);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    var errors = new List<string>();
                    for (int row = 3; row <= rowCount; row++)
                    {
                        try
                        {
                            currentRow = row;
                            var assetGroupHandler = new AssetGroupHandler(_assetRepository, _assetQueryRepository);
                            var assetDetailsHandler = new AssetDetailsHandler(_assetRepository, _assetQueryRepository,_ipAddressService);
                            var assetPurchaseHandler = new AssetPurchaseHandler(worksheet, row,_ipAddressService);
                            var assetInsuranceHandler = new AssetInsuranceHandler(worksheet, row);
                            var assetAdditionalCostHandler = new AssetAdditionalCostHandler(worksheet, row);
                            var assetSpecificationHandler = new AssetSpecificationHandler(worksheet, row);
                            //var assetLocationHandler = new AssetLocationHandler(_assetRepository, _assetQueryRepository); // Pass the correct repositories                        

                            // Extracting all required details
                            var assetDto = await assetGroupHandler.ProcessAssetGroupAsync(request, worksheet, row);
                            await assetDetailsHandler.ProcessAssetDetailsAsync(request, worksheet, row, assetDto);
                            //assetDto.AssetLocation = await assetLocationHandler.ProcessLocationAsync(worksheet, row);                         
                            assetDto.AssetPurchaseDetails = assetPurchaseHandler.ProcessAssetPurchase();
                            assetDto.AssetInsurance = assetInsuranceHandler.ProcessAssetInsurance();
                            assetDto.AssetAdditionalCost = assetAdditionalCostHandler.ProcessAssetAdditionalCost();
                            assetDto.AssetSpecification = assetSpecificationHandler.ProcessSpecifications();

                            assetsDto.Add(assetDto);
                        }
                        
                        catch (Exception ex)
                        {
                            //throw new Exception($"Error at Excel Row {currentRow}: {ex.Message}");
                            return new ApiResponseDTO<bool>
                            {
                                IsSuccess = false,
                                Message = $"Error at Excel Row {currentRow}: {ex.Message}",
                                Data = false
                            };
                        }
                    }
                }
            }
            //return await _assetRepository.ImportAssetsAsync(assetsDto, cancellationToken);
            var result = await _assetRepository.ImportAssetsAsync(assetsDto, cancellationToken);
            return new ApiResponseDTO<bool>
            {
                IsSuccess = result,
                Message = result ? "Assets imported successfully." : "Asset import failed.",
                Data = result
            };
        }
    }
}
