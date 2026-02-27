using System.Data;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contracts
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using Dapper;
using FAM.Infrastructure.Repositories.Common;
using Newtonsoft.Json;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetMasterGeneral
{
    public class AssetMasterGeneralQueryRepository : BaseQueryRepository,IAssetMasterGeneralQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;
        private readonly ICompanyLookup _companyLookup;

        public AssetMasterGeneralQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService,
            IDepartmentLookup departmentLookup, IUnitLookup unitLookup, ICountryLookup countryLookup,
            IStateLookup stateLookup, ICityLookup cityLookup, ICompanyLookup companyLookup)
            : base(ipAddressService)
        {
            _dbConnection = dbConnection;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _countryLookup = countryLookup;
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
            _companyLookup = companyLookup;
        }     
        public async Task<(List<AssetMasterGeneralDTO>, int)> GetAllAssetAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", CompanyId);
            parameters.Add("@UnitId", UnitId);
            parameters.Add("@PageNumber", PageNumber);
            parameters.Add("@PageSize", PageSize);
            parameters.Add("@SearchTerm", string.IsNullOrEmpty(SearchTerm) ? null : SearchTerm);
            using var multiResult = await _dbConnection.QueryMultipleAsync(
            "dbo.FAM_GetAllAssets", parameters, commandType: CommandType.StoredProcedure);            
            // Read the first result set (Paginated Asset List)
            var assetMasterList = (await multiResult.ReadAsync<AssetMasterGeneralDTO>()).ToList();
            // Read the second result set (Total Record Count)
            int totalCount = await multiResult.ReadFirstAsync<int>();
            // Deserialize JSON for Specifications
            foreach (var asset in assetMasterList)
            {
                if (!string.IsNullOrEmpty(asset.SpecificationsJson))
                {
                    asset.Specifications = JsonConvert.DeserializeObject<List<AssetSpecificationDTO>>(asset.SpecificationsJson);
                }
                else
                {
                    asset.Specifications = new List<AssetSpecificationDTO>();
                }
            }
            return (assetMasterList, totalCount);          
        }
        public async Task<List<AssetMasterGeneralDTO>> GetByAssetNameAsync(string searchPattern)
        {
            const string query = @"            
            SELECT AM.Id,AM.CompanyId,AM.UnitId,AM.AssetCode,AM.AssetName,AM.AssetGroupId,AM.AssetSubGroupId,AM.AssetCategoryId,AM.AssetSubCategoryId,AM.AssetParentId,AM.AssetType,AM.MachineCode,AM.Quantity
            ,AM.UOMId,AM.AssetDescription,AM.WorkingStatus,AM.AssetImage,AM.ISDepreciated,AM.IsTangible,AM.IsActive
            ,AM.CreatedBy,AM.CreatedDate,AM.CreatedByName,AM.CreatedIP,AM.ModifiedBy,AM.ModifiedDate,AM.ModifiedByName,AM.ModifiedIP
            ,AG.GroupName AssetGroupName,ASG.SubGroupName,AC.CategoryName AssetCategoryDesc,A.Description AssetSubCategoryDesc,U.UOMName,MM.description WorkingStatusDesc,
            M.description AssetTypeDesc,isnull(AM1.AssetDescription,'') ParentAssetDesc,AM.PutToUseDate
            FROM FixedAsset.AssetMaster AM
            INNER JOIN FixedAsset.AssetGroup AG on AG.Id=AM.AssetGroupId
            LEFT JOIN [FixedAsset].[AssetSubGroup] ASG ON AM.AssetSubGroupId = ASG.Id
            INNER JOIN FixedAsset.AssetCategories AC on AC.Id=AM.AssetCategoryId
            INNER JOIN FixedAsset.AssetSubCategories A on A.Id=AM.AssetSubCategoryId
            INNER JOIN FixedAsset.UOM U on U.Id=AM.UOMId
            INNER JOIN FixedAsset.MiscMaster MM on MM.Id =AM.WorkingStatus
            LEFT JOIN FixedAsset.MiscMaster M on M.Id =AM.AssetType
            LEFT JOIN FixedAsset.AssetMaster AM1 on AM1.Id =AM.AssetParentId
            WHERE AM.CompanyId = @CompanyId AND AM.UnitId = @UnitId AND  (AM.AssetName LIKE @SearchPattern OR AM.AssetCode LIKE @SearchPattern) 
            AND  AM.IsDeleted=0 and AM.IsActive=1 ORDER BY AM.ID DESC";            
            //var result = await _dbConnection.QueryAsync<AssetMasterGeneralDTO>(query, new { SearchPattern = $"%{searchPattern}%" });
             var result = await _dbConnection.QueryAsync<AssetMasterGeneralDTO>(
                query,
                new
                {
                    CompanyId,
                    UnitId,
                    SearchPattern = $"%{searchPattern}%"
                });
            return result.ToList();
        }
       public async Task<AssetMasterGeneralDTO> GetByIdAsync(int assetId)
        {
            const string query = @"            
            SELECT AM.Id, AM.CompanyId, AM.UnitId, AM.AssetCode, AM.AssetName, AM.AssetGroupId, AM.AssetSubGroupId, AM.AssetCategoryId, AM.AssetSubCategoryId, AM.AssetParentId, 
                AM.AssetType, AM.MachineCode, AM.Quantity, AM.UOMId, AM.AssetDescription, AM.WorkingStatus, AM.AssetImage, AM.ISDepreciated, AM.IsTangible, 
                AM.IsActive, AM.CreatedBy, AM.CreatedDate, AM.CreatedByName, AM.CreatedIP, AM.ModifiedBy, AM.ModifiedDate, AM.ModifiedByName, AM.ModifiedIP,
                AG.GroupName AS AssetGroupName,ASG.SubGroupName, AC.CategoryName AS AssetCategoryDesc, A.Description AS AssetSubCategoryDesc, U.UOMName, 
                MM.Description AS WorkingStatusDesc, M.Description AS AssetTypeDesc, ISNULL(AM1.AssetDescription, '') AS ParentAssetDesc,
                (SELECT A.Id AS SpecificationId, A.SpecificationValue, SM.SpecificationName 
                    FROM FixedAsset.AssetSpecifications AS A
                    INNER JOIN FixedAsset.SpecificationMaster SM ON SM.Id = A.SpecificationId    
                    WHERE A.AssetId = AM.Id AND A.IsDeleted = 0 
                    FOR JSON PATH) AS SpecificationsJson   ,AM.PutToUseDate
            FROM FixedAsset.AssetMaster AM
            INNER JOIN FixedAsset.AssetGroup AG ON AG.Id = AM.AssetGroupId
            LEFT JOIN [FixedAsset].[AssetSubGroup] ASG ON AM.AssetSubGroupId = ASG.Id
            INNER JOIN FixedAsset.AssetCategories AC ON AC.Id = AM.AssetCategoryId
            INNER JOIN FixedAsset.AssetSubCategories A ON A.Id = AM.AssetSubCategoryId
            INNER JOIN FixedAsset.UOM U ON U.Id = AM.UOMId
            INNER JOIN FixedAsset.MiscMaster MM ON MM.Id = AM.WorkingStatus
            LEFT JOIN FixedAsset.MiscMaster M ON M.Id = AM.AssetType
            LEFT JOIN FixedAsset.AssetMaster AM1 ON AM1.Id = AM.AssetParentId
            WHERE AM.CompanyId = @CompanyId AND AM.UnitId = @UnitId AND   AM.Id = @assetId AND AM.IsDeleted = 0";

            //var assetMaster = await _dbConnection.<AssetMasterGeneralDTO>(query, new { assetId });
             var assetMaster = await _dbConnection.QueryFirstOrDefaultAsync<AssetMasterGeneralDTO>(
            query,
            new
            {
                CompanyId,
                UnitId,
                assetId = assetId
            });

            if (assetMaster is null)
            {
                throw new KeyNotFoundException($"DepreciationGroup with ID {assetId} not found.");
            }

            // 🔹 Deserialize JSON directly for the single object
            if (!string.IsNullOrEmpty(assetMaster.SpecificationsJson))
            {
                assetMaster.Specifications = JsonConvert.DeserializeObject<List<AssetSpecificationDTO>>(assetMaster.SpecificationsJson) ?? new();
            }
            else
            {
                assetMaster.Specifications = new List<AssetSpecificationDTO>();
            }
            return assetMaster;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetWorkingStatusAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_WorkingStatus.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetAssetTypeAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_AssetType.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }

        public async Task<bool> GetAssetChildDetails(int assetId)
        {
            const string query = @"
                SELECT 1 FROM [FixedAsset].[AssetLocation] WHERE unit_id=@UnitId AND AssetId = @Id ;
                SELECT 1 FROM [FixedAsset].[AssetPurchaseDetails] WHERE AssetId = @Id ;
                SELECT 1 FROM [FixedAsset].[AssetWarranty] WHERE AssetId = @Id AND IsDeleted = 0;
                SELECT 1 FROM [FixedAsset].[AssetSpecifications] WHERE AssetId = @Id AND IsDeleted = 0;
                SELECT 1 FROM [FixedAsset].[AssetAmc] WHERE AssetId = @Id AND IsDeleted = 0;
                SELECT 1 FROM [FixedAsset].[AssetInsurance] WHERE AssetId = @Id AND IsDeleted = 0;
                SELECT 1 FROM [FixedAsset].[AssetAdditionalCost] WHERE AssetId = @Id ;
                SELECT 1 FROM [FixedAsset].[AssetDisposal] WHERE AssetId = @Id AND IsDeleted = 0;
                SELECT 1 FROM [FixedAsset].[DepreciationDetail] WHERE companyId=@CompanyId AND unit_id=@UnitId AND  AssetId = @Id ";

            //using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = assetId });        
            using var multi = await _dbConnection.QueryMultipleAsync(
                query,
                new
                {
                    UnitId,
                    Id = assetId,
                    CompanyId                    
                });            

            var locationExists = await multi.ReadFirstOrDefaultAsync<int?>();  
            var purchaseExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var warrantyExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var specExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var amcExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var insuranceExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var additionalCostExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var depreciationExists = await multi.ReadFirstOrDefaultAsync<int?>();
        
            return locationExists.HasValue || purchaseExists.HasValue || warrantyExists.HasValue  || specExists.HasValue  || amcExists.HasValue || insuranceExists.HasValue || additionalCostExists.HasValue || depreciationExists.HasValue ; 
        }

        public async Task<string?> GetLatestAssetCode( int assetGroupId, int assetCategoryId, int DepartmentId, int LocationId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", CompanyId);
            parameters.Add("@UnitId", UnitId);
            parameters.Add("@GroupId", assetGroupId);
            parameters.Add("@CategoryId", assetCategoryId);
            parameters.Add("@DeptId", DepartmentId);
            parameters.Add("@LocationId", LocationId);
            var newAssetCode = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "dbo.FAM_GetAssetCode", 
                parameters, 
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);
            return newAssetCode; 
        }

        public async Task<string> GetBaseDirectoryAsync()
        {
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "dbo.FAM_GetBaseDirectory", 
                commandType: CommandType.StoredProcedure);
            return result ?? string.Empty; // return an empty string if result is null
        }

        public async  Task<List<FAM.Domain.Entities.MiscMaster>>GetAssetPattern()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP,MiscTypeCode
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
              var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_CodePattern.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();        
        }

        public async Task<AssetMasterGeneralDTO> GetByParentIdAsync(int assetTypeId)
        {
              const string query = @"            
            SELECT AM.Id, AM.AssetCode, AM.AssetName
            FROM FixedAsset.AssetMaster AM           
            WHERE AM.Id = @depGroupId AND AM.IsDeleted = 0";

            var assetMaster = await _dbConnection.QueryFirstOrDefaultAsync<AssetMasterGeneralDTO>(query, new { assetTypeId });

            if (assetMaster is null)
            {
                throw new KeyNotFoundException($"DepreciationGroup with ID {assetTypeId} not found.");
            }

            // 🔹 Deserialize JSON directly for the single object
            if (!string.IsNullOrEmpty(assetMaster.SpecificationsJson))
            {
                assetMaster.Specifications = JsonConvert.DeserializeObject<List<AssetSpecificationDTO>>(assetMaster.SpecificationsJson) ?? new();
            }
            else
            {
                assetMaster.Specifications = new List<AssetSpecificationDTO>();
            }
            return assetMaster;
        }
   
        public async Task<(dynamic AssetResult, dynamic LocationResult, IEnumerable<dynamic> PurchaseDetails, IEnumerable<dynamic> Spec, IEnumerable<dynamic> Warranty, IEnumerable<dynamic> Amc, dynamic Disposal, IEnumerable<dynamic> Insurance, IEnumerable<dynamic> AdditionalCost)> GetAssetMasterByIdAsync(int assetId)
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            var oldUnitLookup = units
                .Where(u => !string.IsNullOrEmpty(u.OldUnitId))
                .GroupBy(u => u.OldUnitId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.First().UnitName ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var CompanyName = companyLookup.TryGetValue(CompanyId, out var cName) ? cName : string.Empty;
            var UnitName = unitLookup.TryGetValue(UnitId, out var uName) ? uName : string.Empty;
            var OldUnitName = oldUnitLookup.TryGetValue(OldUnitId, out var oName) ? oName : string.Empty;

            var sqlQuery = @"
                -- First Query: AssetMaster (One-to-One)
                SELECT AM.AssetName, AM.AssetCode, AM.Quantity, U.UOMName, AG.GroupName,ASG.SubGroupName,AC.CategoryName, ASUBC.SubCategoryName, AssetParent.AssetName ParentName,AM.AssetGroupId ,AM.AssetSubGroupId ,                
                case when (isnull(AM.AssetImage,'') <> '') then MM.Description+''+MM1.Description+'/'+ @companyName + '/' + @unitName +'/'+AM.AssetImage  else 
                '' end AssetImage ,  
                AM.AssetCategoryId,AM.AssetSubCategoryId,
                AM.AssetParentId,AM.AssetType,AM.UOMId,AM.WorkingStatus,AM.AssetImage AssetImageName,
                case when (isnull(AM.AssetDocument,'') <> '') then MM.Description+''+MM2.Description+'/'+ @companyName + '/' + @unitName +'/'+AM.AssetDocument  else 
                '' end AssetDocument ,  AM.AssetDocument AssetDocumentName,AM.PutToUseDate
                FROM [FixedAsset].[AssetMaster] AM
                INNER JOIN [FixedAsset].[UOM] U ON U.Id = AM.UOMId
                INNER JOIN [FixedAsset].[AssetGroup] AG ON AM.AssetGroupId = AG.Id
                LEFT JOIN [FixedAsset].[AssetSubGroup] ASG ON AM.AssetSubGroupId = ASG.Id
                INNER JOIN [FixedAsset].[AssetCategories] AC ON AM.AssetCategoryId = AC.Id
                INNER JOIN [FixedAsset].[AssetSubCategories] ASUBC ON AM.AssetSubCategoryId = ASUBC.Id
                LEFT JOIN [FixedAsset].[AssetMaster] AssetParent ON AM.AssetParentId = AssetParent.Id
                LEFT JOIN FixedAsset.MiscTypeMaster MM on MM.MiscTypeCode ='GETASSETIMAGE'
                LEFT JOIN FixedAsset.MiscTypeMaster MM1 on MM1.MiscTypeCode ='ASSETIMAGE'
                LEFT JOIN FixedAsset.MiscTypeMaster MM2 on MM2.MiscTypeCode ='ASSETDocument'
                WHERE  AM.CompanyId = @CompanyId AND AM.UnitId = @UnitId AND   AM.Id = @AssetId;

                -- Second Query: AssetLocation (One-to-One)
                SELECT @unitName UnitName,'' DeptName,L.LocationName,SL.SubLocationName,@oldUnitId OldUnitId,AL.CustodianId,AL.UserId,AL.DepartmentId,AL.LocationId,AL.SubLocationId
                FROM [FixedAsset].[AssetLocation] AL
                INNER JOIN [FixedAsset].[Location] L ON L.Id=AL.LocationId
                INNER JOIN [FixedAsset].[SubLocation] SL ON SL.Id=AL.SubLocationId                
                WHERE AL.UnitId = @UnitId AND AL.AssetId = @AssetId;                

                -- Third Query: AssetPurchaseDetails (One-to-Many)
                SELECT distinct AP.Id,AP.VendorCode, AP.VendorName,@oldUnitName UnitName,ASource.SourceName,AP.GrnNo,
                CASE  WHEN AP.GrnDate = '0001-01-01' THEN NULL  ELSE CAST(AP.GrnDate AS DATE)  END AS GrnDate ,
                AP.GrnSno,AP.GrnValue,AP.PoNo, CASE  WHEN AP.PoDate = '0001-01-01' THEN NULL  ELSE CAST(AP.PoDate AS DATE)  END AS PoDate,
                AP.PurchaseValue,AP.AcceptedQty,AP.Uom,
                AP.PoSno,AP.ItemCode,AP.ItemName,AP.BillNo,Cast(AP.BillDate AS date) AS BillDate ,AP.BinLocation 
                ,AP.PjYear,AP.PjDocId,AP.PjDocSr,AP.PjDocNo,AP.AssetSourceId ,cast(AP.CapitalizationDate as date)CapitalizationDate
                FROM [FixedAsset].[AssetPurchaseDetails] AP                
                INNER JOIN [FixedAsset].[AssetSource] ASource ON ASource.Id=AP.AssetSourceId
                WHERE  AP.OldUnitId=@oldUnitId AND AP.AssetId = @AssetId;

                SELECT A.Id,SM.SpecificationName,A.SpecificationValue,A.SpecificationId,SM.IsDefault FROM  [FixedAsset].[AssetSpecifications] A
                INNER JOIN [FixedAsset].[SpecificationMaster] SM ON SM.Id=A.SpecificationId
                WHERE A.AssetId=@AssetId  and A.IsDeleted=0

                SELECT Aw.Id,CAST(AW.StartDate AS DATE) AS StartDate,CAST(AW.EndDate AS DATE) AS EndDate,AW.Period,MMWaranty.description AS WarrantyType,MMClaim.description AS ServiceClaimStatus,
                AW.WarrantyProvider,AW.MobileNumber,AW.ContactPerson,AW.Description,AW.Email,AW.Document,
                CAST(NULL AS NVARCHAR(200)) AS CountryName,CAST(NULL AS NVARCHAR(200)) AS StateName,CAST(NULL AS NVARCHAR(200)) AS CityName,
                AW.ServiceAddressLine1,AW.ServiceAddressLine2,
                AW.ServicePinCode,AW.ServiceContactPerson,AW.ServiceMobileNumber,AW.ServiceEmail,AW.ServiceClaimProcessDescription,
                CAST(AW.ServiceLastClaimDate AS DATE) AS ServiceLastClaimDate,AW.WarrantyType AS WarrantyTypeId,
                AW.ServiceClaimStatus AS ServiceClaimStatusId,AW.ServiceCountryId,AW.ServiceStateId,AW.ServiceCityId
                FROM [FixedAsset].[AssetWarranty] AW
                INNER JOIN [FixedAsset].[MiscMaster] MMWaranty ON MMWaranty.Id=AW.WarrantyType
                LEFT JOIN [FixedAsset].[MiscMaster] MMClaim ON MMClaim.Id=AW.ServiceClaimStatus
                WHERE AW.AssetId=@AssetId  and AW.IsDeleted=0

                SELECT AA.Id,CAST(AA.StartDate AS DATE) AS StartDate,CAST(AA.EndDate AS DATE) AS EndDate,AA.Period,AA.VendorCode,AA.VendorName,
                MMCoverage.description AS CoverageType,
                MMRenewal.description AS RenewalStatus,CAST(AA.RenewedDate AS DATE) AS RenewedDate,AA.CoverageType AS CoverageTypeId,
                AA.RenewalStatus AS RenewalStatusId,AA.IsActive,AA.FreeServiceCount,AA.VendorEmail,AA.VendorPhone
                FROM [FixedAsset].[AssetAmc] AA
                INNER JOIN [FixedAsset].[MiscMaster] MMCoverage ON MMCoverage.Id=AA.CoverageType
                INNER JOIN [FixedAsset].[MiscMaster] MMRenewal ON MMRenewal.Id=AA.RenewalStatus
                WHERE AA.AssetId=@AssetId  and AA.IsDeleted=0

                SELECT AD.Id,MMDisposal.description AS DisposalType,CAST(AD.DisposalDate AS DATE) AS DisposalDate,AD.DisposalReason,
                AD.DisposalAmount,AD.DisposalType AS DisposalTypeId  ,AD.AssetPurchaseId
                FROM [FixedAsset].[AssetDisposal] AD
                INNER JOIN [FixedAsset].[MiscMaster] MMDisposal ON MMDisposal.Id=AD.DisposalType
                WHERE AD.AssetId=@AssetId  and AD.IsDeleted=0

                SELECT AI.Id, PolicyNo,CAST(StartDate AS DATE) AS StartDate,CAST(EndDate AS DATE) AS EndDate,Insuranceperiod,PolicyAmount,
                VendorCode,MM.Code RenewalStatus,CAST(RenewedDate AS DATE) AS RenewedDate,AI.IsActive
                FROM [FixedAsset].[AssetInsurance] AI
                INNER JOIN [FixedAsset].[MiscMaster] MM ON MM.Id=AI.RenewalStatus
                WHERE AssetId=@AssetId and  AI.IsDeleted=0

                SELECT AC.Id,AssetSourceId,Amount,JournalNo,CostType,MM.Code CostTypeDesc
                FROM [FixedAsset].[AssetAdditionalCost]AC
                inner join FixedAsset.MiscMaster MM on MM.id=CostType 
                WHERE AssetId=@AssetId  
                ";

            //using var multi = await _dbConnection.QueryMultipleAsync(sqlQuery, new { AssetId = assetId });

            using var multi = await _dbConnection.QueryMultipleAsync(
                sqlQuery,
                new
                {
                    CompanyId,
                    UnitId,
                    AssetId = assetId
                     ,companyName = CompanyName,
                     unitName = UnitName, oldUnitId = OldUnitId,oldUnitName=OldUnitName
                });            


            var assetResult     = (await multi.ReadAsync<dynamic>()).FirstOrDefault();
            var locationResult  = (await multi.ReadAsync<dynamic>()).FirstOrDefault();
            var purchaseDetails = (await multi.ReadAsync<dynamic>()).ToList();
            var specDetails     = (await multi.ReadAsync<dynamic>()).ToList();
            var warrantyDetails = (await multi.ReadAsync<dynamic>()).ToList();
            var amcDetails      = (await multi.ReadAsync<dynamic>()).ToList();
            var disposalResult  = (await multi.ReadAsync<dynamic>()).FirstOrDefault();
            var insuranceDetails = (await multi.ReadAsync<dynamic>()).ToList();
            var additionalCost   = (await multi.ReadAsync<dynamic>()).ToList();

            // Enrich warranty details with Country, State, City names via lookups
            if (warrantyDetails.Count > 0)
            {
                var countryIds = warrantyDetails
                    .Select(w => (int)w.ServiceCountryId)
                    .Where(x => x > 0).Distinct().ToArray();
                var stateIds = warrantyDetails
                    .Select(w => (int)w.ServiceStateId)
                    .Where(x => x > 0).Distinct().ToArray();
                var cityIds = warrantyDetails
                    .Select(w => (int)w.ServiceCityId)
                    .Where(x => x > 0).Distinct().ToArray();

                var countryMap = new Dictionary<int, string>();
                var stateMap = new Dictionary<int, string>();
                var cityMap = new Dictionary<int, string>();

                if (countryIds.Length > 0)
                {
                    var countries = await _countryLookup.GetByIdsAsync(countryIds);
                    countryMap = countries.Where(c => c != null).ToDictionary(c => c.CountryId, c => c.CountryName);
                }
                if (stateIds.Length > 0)
                {
                    var states = await _stateLookup.GetByIdsAsync(stateIds);
                    stateMap = states.Where(s => s != null).ToDictionary(s => s.StateId, s => s.StateName);
                }
                if (cityIds.Length > 0)
                {
                    var cities = await _cityLookup.GetByIdsAsync(cityIds);
                    cityMap = cities.Where(c => c != null).ToDictionary(c => c.CityId, c => c.CityName);
                }

                foreach (var w in warrantyDetails)
                {
                    if (countryMap.TryGetValue((int)w.ServiceCountryId, out var countryName))
                        w.CountryName = countryName;
                    if (stateMap.TryGetValue((int)w.ServiceStateId, out var stateName))
                        w.StateName = stateName;
                    if (cityMap.TryGetValue((int)w.ServiceCityId, out var cityName))
                        w.CityName = cityName;
                }
            }

        if (locationResult != null)
        {
             if (departmentLookup.TryGetValue(locationResult.DepartmentId, out string deptName))
                 {
                 locationResult.DeptName = deptName;
             }
        
            // Fetch Custodian
            if (!string.IsNullOrEmpty(locationResult.OldUnitId) && locationResult.CustodianId > 0)
            {
                var custodian = await _dbConnection.QueryFirstOrDefaultAsync<Employee>(
                    "dbo.GetEmployeeByDivision",
                    new { DivCode = locationResult.OldUnitId, EmpNo = locationResult.CustodianId },
                    commandType: CommandType.StoredProcedure
                );

                if (custodian != null)
                    locationResult.CustodianName = custodian.Empname;
            }
            // Fill User Name
            if (!string.IsNullOrEmpty(locationResult.OldUnitId) && locationResult.UserId > 0)
            {
                var user = await _dbConnection.QueryFirstOrDefaultAsync<Employee>(
                    "dbo.GetEmployeeByDivision",
                    new { DivCode = locationResult.OldUnitId, EmpNo = locationResult.UserId },
                    commandType: CommandType.StoredProcedure
                );

                if (user != null)
                    locationResult.UserName = user.Empname;
            }           
        }
        return (assetResult!, locationResult!, purchaseDetails, specDetails, warrantyDetails, amcDetails, disposalResult!, insuranceDetails,additionalCost);
        }

        public async Task<(dynamic AssetResult, dynamic LocationResult, IEnumerable<dynamic> PurchaseDetails, IEnumerable<dynamic> AdditionalCost)> GetAssetMasterSplitByIdAsync(int assetId)
        {
             var companies = await _companyLookup.GetAllCompanyAsync();
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            var oldUnitLookup = units
                .Where(u => !string.IsNullOrEmpty(u.OldUnitId))
                .GroupBy(u => u.OldUnitId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.First().UnitName ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var CompanyName = companyLookup.TryGetValue(CompanyId, out var cName) ? cName : string.Empty;
            var UnitName = unitLookup.TryGetValue(UnitId, out var uName) ? uName : string.Empty;
            var OldUnitName = oldUnitLookup.TryGetValue(OldUnitId, out var oName) ? oName : string.Empty;

            var sqlQuery = @"
                -- First Query: AssetMaster (One-to-One)
                SELECT AM.AssetName, AM.AssetCode, AM.Quantity, U.UOMName, AG.GroupName,ASG.SubGroupName,AC.CategoryName, ASUBC.SubCategoryName, AssetParent.AssetName,AM.AssetGroupId ,AM.AssetSubGroupId,                
                case when (isnull(AM.AssetImage,'') <> '') then MM.Description+''+MM1.Description+'/'+@companyName+'/'+@unitName +'/'+AM.AssetImage  else 
                '' end AssetImage ,  
                AM.AssetCategoryId,AM.AssetSubCategoryId,
                AM.AssetParentId,AM.AssetType,AM.UOMId,AM.WorkingStatus,AM.AssetImage AssetImageName,
                case when (isnull(AM.AssetDocument,'') <> '') then MM.Description+''+MM2.Description+'/'+@companyName+'/'+@unitName  +'/'+AM.AssetDocument  else 
                '' end AssetDocument ,  AM.AssetDocument AssetDocumentName,AM.PutToUseDate
                FROM [FixedAsset].[AssetMaster] AM
                LEFT JOIN [FixedAsset].[UOM] U ON U.Id = AM.UOMId
                INNER JOIN [FixedAsset].[AssetGroup] AG ON AM.AssetGroupId = AG.Id
                LEFT JOIN [FixedAsset].[AssetSubGroup] ASG ON AM.AssetSubGroupId = ASG.Id
                INNER JOIN [FixedAsset].[AssetCategories] AC ON AM.AssetCategoryId = AC.Id
                INNER JOIN [FixedAsset].[AssetSubCategories] ASUBC ON AM.AssetSubCategoryId = ASUBC.Id
                LEFT JOIN [FixedAsset].[AssetMaster] AssetParent ON AM.AssetParentId = AssetParent.Id                
                LEFT JOIN FixedAsset.MiscTypeMaster MM on MM.MiscTypeCode ='GETASSETIMAGE'               
                LEFT JOIN FixedAsset.MiscTypeMaster MM1 on MM1.MiscTypeCode ='ASSETIMAGE'
                LEFT JOIN FixedAsset.MiscTypeMaster MM2 on MM2.MiscTypeCode ='ASSETDocument'
                WHERE AM.CompanyId = @CompanyId AND AM.UnitId = @UnitId AND AM.Id = @AssetId;

                -- Second Query: AssetLocation (One-to-One)
                SELECT @unitName UnitName,'' DeptName,L.LocationName,SL.SubLocationName,@oldUnitId as OldUnitId,AL.CustodianId,AL.UserId,AL.DepartmentId,AL.LocationId,AL.SubLocationId
                FROM [FixedAsset].[AssetLocation] AL
                INNER JOIN [FixedAsset].[Location] L ON L.Id=AL.LocationId
                INNER JOIN [FixedAsset].[SubLocation] SL ON SL.Id=AL.SubLocationId                            
                WHERE AL.UnitId = @UnitId AND  AL.AssetId = @AssetId;
                

                -- Third Query: AssetPurchaseDetails (One-to-Many)
                SELECT distinct AP.Id,AP.VendorCode, AP.VendorName,@oldUnitId UnitName,ASource.SourceName,AP.GrnNo,
                CASE  WHEN AP.GrnDate = '0001-01-01' THEN NULL  ELSE CAST(AP.GrnDate AS DATE)  END AS GrnDate,                
                AP.GrnSno,AP.GrnValue,AP.PoNo,
                CASE  WHEN AP.PoDate = '0001-01-01' THEN NULL  ELSE CAST(AP.PoDate AS DATE)  END AS PoDate,
                AP.PurchaseValue,AP.AcceptedQty,AP.Uom,
                AP.PoSno,AP.ItemCode,AP.ItemName,AP.BillNo,Cast(AP.BillDate AS date) AS BillDate ,AP.BinLocation 
                ,AP.PjYear,AP.PjDocId,AP.PjDocSr,AP.PjDocNo,AP.AssetSourceId ,cast(AP.CapitalizationDate as varchar)CapitalizationDate
                FROM [FixedAsset].[AssetPurchaseDetails] AP                
                INNER JOIN [FixedAsset].[AssetSource] ASource ON ASource.Id=AP.AssetSourceId
                WHERE  AP.OldUnitId=@oldUnitId  AND AP.AssetId = @AssetId;             

                SELECT AC.Id,AssetSourceId,Amount,JournalNo,CostType,MM.Code CostTypeDesc
                FROM [FixedAsset].[AssetAdditionalCost]AC
                inner join FixedAsset.MiscMaster MM on MM.id=CostType 
                WHERE AssetId=@AssetId
                ";

            //using var multi = await _dbConnection.QueryMultipleAsync(sqlQuery, new { AssetId = assetId });

            using var multi = await _dbConnection.QueryMultipleAsync(
                sqlQuery,
                new
                {
                    CompanyId,
                    UnitId,
                    AssetId = assetId
                     ,companyName = CompanyName,
                     unitName = UnitName,
                     oldUnitId = OldUnitId,
                     oldUnitName = OldUnitName
                });    
            var assetResult     = (await multi.ReadAsync<dynamic>()).FirstOrDefault();
            var locationResult  = (await multi.ReadAsync<dynamic>()).FirstOrDefault();
            var purchaseDetails = (await multi.ReadAsync<dynamic>()).ToList();
            var additionalCost   = (await multi.ReadAsync<dynamic>()).ToList();

            if (locationResult != null)
            {
                 if (departmentLookup.TryGetValue(locationResult.DepartmentId, out string deptName))
                 {
                     locationResult.DeptName = deptName;
                 }
                // Fetch Custodian
                if (!string.IsNullOrEmpty(locationResult.OldUnitId) && locationResult.CustodianId > 0)
                {                   

                    var custodianEmployee = await _dbConnection.QueryFirstOrDefaultAsync<Employee>(
                        "dbo.GetEmployeeByDivision",
                        new { DivCode = locationResult.OldUnitId, EmpNo = locationResult.CustodianId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (custodianEmployee != null)
                        locationResult.CustodianName = custodianEmployee.Empname;
                }

                // Fetch User
                if (!string.IsNullOrEmpty(locationResult.OldUnitId) && locationResult.UserId > 0)
                {
                    var user = await _dbConnection.QueryFirstOrDefaultAsync<Employee>(
                        "dbo.GetEmployeeByDivision",
                        new { DivCode = locationResult.OldUnitId, EmpNo = locationResult.UserId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (user != null)
                        locationResult.UserName = user.Empname;
                }   
            }            
            return (assetResult!, locationResult!, purchaseDetails, additionalCost);
       
        }       

        public async Task<string> GetDocumentDirectoryAsync()
        {
            const string query = @"
            SELECT Description            
            FROM FixedAsset.MiscTypeMaster             
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  IsDeleted=0 and IsActive=1
            ORDER BY ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.AssetDocumentImage.MiscCode };        
            var result = await _dbConnection.QueryAsync<string>(query,parameters);
            return result.FirstOrDefault() ?? string.Empty;
        }
    }
}
