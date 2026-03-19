using System.Data;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MaintenanceManagement.Domain.Common;
using Dapper;
using Serilog;

namespace MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest
{
    public class MaintenanceRequestQueryRepository : IMaintenanceRequestQueryRepository
    {
                private readonly IDbConnection _dbConnection; 

                private readonly IIPAddressService _iPAddressService;
           

                public MaintenanceRequestQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService )
        {
            _dbConnection = dbConnection;
            _iPAddressService = iPAddressService;
        }             


                public async Task<(IEnumerable<dynamic> MaintenanceRequestList, int)> GetAllMaintenanceRequestAsync(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? FromDate,    DateTimeOffset? ToDate   ) 
        {
            var UnitId= _iPAddressService.GetUnitId() ?? 0;
                                var query = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Maintenance.MaintenanceRequest A
                 INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id 
                LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id    
                LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id  
                LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id 
                LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id 
                LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id   
                inner  join Maintenance.WorkOrder WO  on a.id=WO.RequestId 
                LEFT JOIN Maintenance.MiscMaster k on WO.StatusId=k.Id                       
                WHERE 
                A.IsDeleted = 0   AND  B.Code = @MiscCode  AND J.Code <> @MaintenanceStatusUpdate
                and cast(A.createddate as date) >=@FromDate and cast(A.createddate as date) <= @ToDate AND A.UnitId = @UnitId                      
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(A.Id AS NVARCHAR) LIKE @Search OR A.Remarks LIKE @Search OR B.Code LIKE @Search OR C.Code LIKE @Search  OR F.Code LIKE @Search OR G.Code LIKE @Search OR E.MachineName LIKE @Search OR H.Code LIKE @Search OR I.Code LIKE @Search OR J.Code LIKE @Search ) ")}};

                SELECT 
                    A.Id,
                    A.ProductionDepartmentId AS ProductionDepartmentId,
                    A.MaintenanceDepartmentId AS MaintenanceDepartmentId,
                    A.SourceId,
                    A.VendorId,
                    A.VendorName,
                    A.OldVendorId,
                    A.OldVendorName,
                    A.Remarks,
                    A.CreatedByName,
                    A.CreatedDate,
                    A.CreatedBy,
                    A.CreatedIP,
                    A.ModifiedByName,
                    A.ModifiedDate,
                    A.ModifiedBy,
                    A.ModifiedIP,
                    B.Id AS RequestTypeId,
                    B.Code AS RequestType,
                    C.Id AS MaintenanceTypeId,
                    C.Code AS MaintenanceType,
                    E.Id AS MachineId,
                    E.MachineName AS MachineName,
                    F.Id AS ServiceTypeId,
                    F.Code AS ServiceType,
                    Cast(A.ExpectedDispatchDate AS Date) AS ExpectedDispatchDate,
                    G.Id AS ServiceLocationId,
                    G.Code AS ServiceLocation,
                    H.Id AS ModeOfDispatchId,
                    H.Code AS ModeOfDispatch,
                    I.Id AS SparesTypeId,
                    I.Code AS SparesType,
                    J.Id AS RequestStatusId,
                    J.Code AS RequestStatus,
                    k.Code  AS  WorkorderStatus,
                    A.Remarks,
                    WO.Id as WorkOrderId

                FROM Maintenance.MaintenanceRequest A
                INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id 
                LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id    
                LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id  
                LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id 
                LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id 
                LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id   
                inner  join Maintenance.WorkOrder WO  on a.id=WO.RequestId 
                LEFT JOIN Maintenance.MiscMaster k on WO.StatusId=k.Id    

                WHERE A.IsDeleted = 0   AND  B.Code = @MiscCode  AND J.Code <> @MaintenanceStatusUpdate                        
                and cast(A.createddate as date) >=@FromDate and cast(A.createddate as date) <= @ToDate
                AND A.UnitId = @UnitId

                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(A.Id AS NVARCHAR) LIKE @Search OR A.Remarks LIKE @Search OR B.Code LIKE @Search OR C.Code LIKE @Search OR F.Code LIKE @Search or G.Code LIKE @Search OR H.Code LIKE @Search  OR E.MachineName LIKE @Search OR I.Code LIKE @Search  or J.Code LIKE @Search or k.Code LIKE @Search ) ")}}
                ORDER BY A.Id DESC 
                 OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;               

                SELECT @TotalCount AS TotalCount;
                """;
            var parameters = new
            {
               //  MiscTypeCode = MiscEnumEntity.MaintenanceRequestType.MiscCode,
                MiscCode = MiscEnumEntity.MaintenanceRequestTypeInternal.Code,
                MaintenanceStatusUpdate = MiscEnumEntity.MaintenanceStatusUpdate.Code,
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                FromDate = FromDate?.Date,                        
                ToDate = ToDate?.Date,                        
                //ToDate = ToDate?.Date.AddDays(1),
                UnitId


            };

                 using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
               var maintenanceReqList = await multi.ReadAsync<dynamic>();
               var totalCount = await multi.ReadFirstAsync<int>();


            return (maintenanceReqList, totalCount);
        }

        // External Request
        public async Task<(IEnumerable<dynamic> MaintenanceRequestList, int)> GetAllMaintenanceExternalRequestAsync(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? FromDate, DateTimeOffset? ToDate)
        {

            var UnitId = _iPAddressService.GetUnitId() ?? 0;
            var query = $$"""
                        DECLARE @TotalCount INT;

                        SELECT @TotalCount = COUNT(*)
                        FROM Maintenance.MaintenanceRequest A
                        INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                        INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                        INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id
                        LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id
                        LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id  
                        LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id  
                         LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id 
                         LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id 
                         LEFT JOIN Maintenance.WorkOrder K ON A.Id = K.RequestId

                        WHERE   B.Code = @MiscCode  AND C.Code <> @MaintenanceStatusUpdate 
                        AND cast(A.createddate as date) >=@FromDate and cast(A.createddate as date) <= @ToDate
                        AND A.UnitId = @UnitId 
                        AND A.Id NOT IN (SELECT RequestId FROM Maintenance.WorkOrder WHERE K.RequestId IS NOT NULL)
                        {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(A.Id AS NVARCHAR) LIKE @Search OR A.Remarks LIKE @Search OR B.Code LIKE @Search OR C.Code LIKE @Search  OR F.Code LIKE @Search OR G.Code LIKE @Search OR E.MachineName LIKE @Search OR H.Code LIKE @Search OR I.Code LIKE @Search OR J.Code LIKE @Search ) ")}};
                                            
                        SELECT 
                            A.Id,
                           A.ProductionDepartmentId AS ProductionDepartmentId,
                            A.MaintenanceDepartmentId AS MaintenanceDepartmentId,
                            A.SourceId,
                            A.VendorId,
                            A.VendorName,
                            A.OldVendorId,
                            A.OldVendorName,
                            A.Remarks,
                            A.EstimatedServiceCost,
                            A.EstimatedSpareCost,
                             A.CreatedByName,
                            A.CreatedDate,
                            A.CreatedBy,
                            A.CreatedIP,
                             A.ModifiedByName,
                            A.ModifiedDate,
                            A.ModifiedBy,
                            A.ModifiedIP,
                            B.Id AS RequestTypeId,
                            B.Code AS RequestType,
                            C.Id AS MaintenanceTypeId,
                            C.Code AS MaintenanceType,
                            E.Id AS MachineId,
                            E.MachineName AS MachineName,
                            F.Id AS ServiceTypeId,
                            F.Code AS ServiceType,
                            Cast(A.ExpectedDispatchDate AS Date) AS ExpectedDispatchDate,
                            G.Id AS ServiceLocationId,
                            G.Code AS ServiceLocation,
                            H.Id AS ModeOfDispatchId,
                            H.Code AS ModeOfDispatch,
                            I.Id AS SparesTypeId,
                            I.Code AS SparesType,
                            J.Id AS RequestStatusId,
                            J.Code AS RequestStatus,
                            L.Code as WorkOrderStatus


                        FROM Maintenance.MaintenanceRequest A
                        INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                        INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                        INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id 
                        LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id    
                        LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id  
                        LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id 
                        LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id 
                        LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id 
                        LEFT JOIN Maintenance.WorkOrder K ON A.Id = K.RequestId
                        inner  join Maintenance.WorkOrder WO  on a.id=WO.RequestId 
                        LEFT JOIN Maintenance.MiscMaster L on WO.StatusId=L.Id      

                        WHERE  B.Code = @MiscCode  AND C.Code <> @MaintenanceStatusUpdate 
                        and cast(A.createddate as date) >=@FromDate and cast(A.createddate as date) <= @ToDate
                        AND A.UnitId = @UnitId 
                        AND A.Id NOT IN (SELECT    RequestId FROM Maintenance.WorkOrder WHERE  K.RequestId IS NOT NULL)
                        {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CAST(A.Id AS NVARCHAR) LIKE @Search OR A.Remarks LIKE @Search OR B.Code LIKE @Search OR C.Code LIKE @Search OR F.Code LIKE @Search or G.Code LIKE @Search OR H.Code LIKE @Search  OR E.MachineName LIKE @Search OR I.Code LIKE @Search  or J.Code LIKE @Search  or L.Code LIKE @Search)")}}
                        ORDER BY A.Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;     

                        SELECT @TotalCount AS TotalCount;
                        """;
            var parameters = new
            {
                //  MiscTypeCode = MiscEnumEntity.MaintenanceRequestType.MiscCode,
                MiscCode = MiscEnumEntity.MaintenanceRequestTypeExternal.Code,
                MaintenanceStatusUpdate = MiscEnumEntity.MaintenanceStatusUpdate.Code,
                MiscType = MiscEnumEntity.WOStatus.MiscCode,
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                FromDate = FromDate?.Date,
                ToDate = ToDate?.Date,
                //ToDate = ToDate?.Date.AddDays(1),
                UnitId
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var maintenanceReqList = await multi.ReadAsync<dynamic>();
            var totalCount = await multi.ReadFirstAsync<int>();


            return (maintenanceReqList, totalCount);
        }


          
                public async Task<dynamic?> GetByIdAsync(int id)
                {

                    var UnitId= _iPAddressService.GetUnitId() ?? 0;
                    var query = @"
                        SELECT 
                            A.Id,
                            A.ProductionDepartmentId AS ProductionDepartmentId,
                            A.MaintenanceDepartmentId AS MaintenanceDepartmentId,
                            A.SourceId,
                            A.VendorId,
                            A.VendorName,
                            A.OldVendorId,
                            A.OldVendorName,
                            A.Remarks,
                            A.EstimatedServiceCost,
                            A.EstimatedSpareCost,
                            A.CreatedByName,
                            A.CreatedDate,
                            A.CreatedBy,
                            A.CreatedIP,
                            A.ModifiedByName,
                            A.ModifiedDate,
                            A.ModifiedBy,
                            A.ModifiedIP,
                            B.Id AS RequestTypeId,
                            B.Code AS RequestType,
                            C.Id AS MaintenanceTypeId,
                            C.Code AS MaintenanceType,
                            E.Id AS MachineId,
                            E.MachineName AS MachineName,
                            F.Id AS ServiceTypeId,
                            F.Code AS ServiceType,
                            CAST(A.ExpectedDispatchDate AS DATE) AS ExpectedDispatchDate,
                            G.Id AS ServiceLocationId,
                            G.Code AS ServiceLocation,
                            H.Id AS ModeOfDispatchId,
                            H.Code AS ModeOfDispatch,
                            I.Id AS SparesTypeId,
                            I.Code AS SparesType,
                            J.Id AS RequestStatusId,
                            J.Code AS RequestStatus,
                            k.Code as WorkorderStatus

                        FROM Maintenance.MaintenanceRequest A
                        INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                        INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                        INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id
                        LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id
                        LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id
                        LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id
                        LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id
                        LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id
                        inner  join Maintenance.WorkOrder WO  on a.id=WO.RequestId 
                        LEFT JOIN Maintenance.MiscMaster k on WO.StatusId=k.Id    

                        WHERE A.IsDeleted = 0 AND A.Id = @Id 
                        AND B.Code = @MiscCode AND A.UnitId = @UnitId;  
                    ";

                   // var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(query, new { Id = id });
                    
                     var parameters = new
                    {
                        Id = id,
                        MiscCode = MiscEnumEntity.MaintenanceRequestTypeInternal.Code,
                         MiscStatusCode = MiscEnumEntity.MaintenanceStatusUpdate.Code,
                        UnitId
                        
                    };

                     var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(query, parameters);

                    return result;
                }

                public async Task<List<GetExternalRequestByIdDto>> GetExternalRequestByIdAsync(List<int> ids)
                {
                    var UnitId= _iPAddressService.GetUnitId() ?? 0;
                    var query = @"
                        SELECT 
                            A.Id,
                           A.ProductionDepartmentId AS ProductionDepartmentId,
                            A.MaintenanceDepartmentId AS MaintenanceDepartmentId,
                            A.SourceId,
                            A.VendorId,
                            A.VendorName,
                            A.OldVendorId,
                            A.OldVendorName,
                            A.Remarks,
                            A.CompanyId,
                            A.UnitId,
                             A.EstimatedServiceCost,
                            A.EstimatedSpareCost,
                             A.CreatedByName,
                            A.CreatedDate,
                            A.CreatedBy,
                            A.CreatedIP,
                             A.ModifiedByName,
                            A.ModifiedDate,
                            A.ModifiedBy,
                            A.ModifiedIP,
                            B.Id AS RequestTypeId,
                            B.Code AS RequestType,
                            C.Id AS MaintenanceTypeId,
                            C.Code AS MaintenanceType,
                            E.Id AS MachineId,
                            E.MachineName AS MachineName,
                            F.Id AS ServiceTypeId,
                            F.Code AS ServiceType,
                            CAST(A.ExpectedDispatchDate AS DATE) AS ExpectedDispatchDate,
                            G.Id AS ServiceLocationId,
                            G.Code AS ServiceLocation,
                            H.Id AS ModeOfDispatchId,
                            H.Code AS ModeOfDispatch,
                            I.Id AS SparesTypeId,
                            I.Code AS SparesType,
                            J.Id AS RequestStatusId,
                            J.Code AS RequestStatus,
                            L.Id as WorkOrderStatus
                        FROM Maintenance.MaintenanceRequest A
                        INNER JOIN Maintenance.MiscMaster B ON A.RequestTypeId = B.Id
                        INNER JOIN Maintenance.MiscMaster C ON A.MaintenanceTypeId = C.Id
                        INNER JOIN Maintenance.MachineMaster E ON A.MachineId = E.Id
                        LEFT JOIN Maintenance.MiscMaster F ON A.ServiceTypeId = F.Id
                        LEFT JOIN Maintenance.MiscMaster G ON A.ServiceLocationId = G.Id
                        LEFT JOIN Maintenance.MiscMaster H ON A.ModeOfDispatchId = H.Id
                        LEFT JOIN Maintenance.MiscMaster I ON A.SparesTypeId = I.Id
                        LEFT JOIN Maintenance.MiscMaster J ON A.RequestStatusId = J.Id
                        INNER JOIN Maintenance.MiscTypeMaster K ON J.MiscTypeId = K.Id
                         inner  join Maintenance.WorkOrder WO  on a.id=WO.RequestId 
                        LEFT JOIN Maintenance.MiscMaster L on WO.StatusId=L.Id    
                        WHERE  A.Id IN @Ids  
                        AND B.Code = @MiscCode AND J.Code <> @MiscStatusCode AND A.UnitId = @UnitId AND K.MiscTypeCode =@MiscType ;
                    ";
                //    var result = await _dbConnection.QueryAsync<GetExternalRequestByIdDto>(query, new { Ids = ids });
                 var parameters = new
                    {
                        Ids = ids,
                        MiscCode = MiscEnumEntity.MaintenanceRequestTypeExternal.Code,
                        MiscStatusCode = MiscEnumEntity.MaintenanceStatusUpdate.Code,
                        MiscType= MiscEnumEntity.WOStatus.MiscCode,
                        UnitId
                       
                    };

                  //   var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(query, parameters);
                   var result = await _dbConnection.QueryAsync<GetExternalRequestByIdDto>(query, parameters);
                    return result.ToList(); // always return a list (empty if nothing found)
                }  
         

                 public async Task<List<MaintenanceManagement.Domain.Entities.ExistingVendorDetails>> GetVendorDetails(string OldUnitId, string? VendorCode)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OldUnitId", OldUnitId, DbType.String);

                // Correctly pass NULL or wildcard pattern
                if (!string.IsNullOrWhiteSpace(VendorCode))
                {
                    parameters.Add("@Slcode", VendorCode, DbType.String); // No wildcard in C# - handled in SQL
                }
                else
                {
                    parameters.Add("@Slcode", DBNull.Value, DbType.String);
                }

                var vendorDetailsList = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.ExistingVendorDetails>(
                    "dbo.GetVendorDetails", 
                    parameters, 
                    commandType: CommandType.StoredProcedure
                );

                if (!vendorDetailsList.Any())
                {
                    Log.Information("No data returned from stored procedure!");
                }

                    return vendorDetailsList?.ToList() ?? new List<MaintenanceManagement.Domain.Entities.ExistingVendorDetails>();
            }


               public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenancestatusAsync()
                {
                    const string query = @"
                        SELECT M.Id, MiscTypeId, Code, M.Description, SortOrder, M.IsActive,
                            M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP,
                            M.ModifiedBy, M.ModifiedDate, M.ModifiedByName, M.ModifiedIP
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T ON T.ID = M.MiscTypeId
                        WHERE T.MiscTypeCode = @MiscTypeCode AND M.Code = @MiscCode
                        AND M.IsDeleted = 0 AND M.IsActive = 1
                        ORDER BY M.ID DESC";

                    var parameters = new
                    {
                        MiscTypeCode = MiscEnumEntity.WOStatus.MiscCode,
                        MiscCode = MiscEnumEntity.MaintenanceStatusUpdate.Code
                    };

                    var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
                    return result.ToList();
                }

                public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceOpenstatusAsync()
                {
                    const string query = @"
                        SELECT M.Id, MiscTypeId, Code, M.Description, SortOrder, M.IsActive,
                            M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP,
                            M.ModifiedBy, M.ModifiedDate, M.ModifiedByName, M.ModifiedIP
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T ON T.ID = M.MiscTypeId
                        WHERE T.MiscTypeCode = @MiscTypeCode AND M.Code = @MiscCode
                        AND M.IsDeleted = 0 AND M.IsActive = 1
                        ORDER BY M.ID DESC";

                    var parameters = new
                    {
                        MiscTypeCode = MiscEnumEntity.WOStatus.MiscCode,
                        MiscCode = MiscEnumEntity.StatusOpen.Code
                    };

                    var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
                    return result.ToList();
                }

                public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceRequestTypeAsync()
                {
                    const string query = @"
                        SELECT M.Id, MiscTypeId, Code, M.Description, SortOrder, M.IsActive,
                            M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP,
                            M.ModifiedBy, M.ModifiedDate, M.ModifiedByName, M.ModifiedIP
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T ON T.ID = M.MiscTypeId
                        WHERE  M.Code = @MiscCode AND T.MiscTypeCode = @MiscTypeCode
                        AND M.IsDeleted = 0 AND M.IsActive = 1
                        ORDER BY M.ID DESC";

                    var parameters = new
                    {
                        MiscTypeCode = MiscEnumEntity.MaintenanceRequestType.MiscCode,
                        MiscCode = MiscEnumEntity.MaintenanceRequestTypeInternal.Code
                    };

                    var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
                    return result.ToList();
                }

                 public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceExternalRequestTypeAsync()
                {
                    const string query = @"
                        SELECT M.Id, MiscTypeId, Code, M.Description, SortOrder, M.IsActive,
                            M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP,
                            M.ModifiedBy, M.ModifiedDate, M.ModifiedByName, M.ModifiedIP
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T ON T.ID = M.MiscTypeId
                        WHERE  M.Code = @MiscCode AND T.MiscTypeCode = @MiscTypeCode
                        AND M.IsDeleted = 0 AND M.IsActive = 1
                        ORDER BY M.ID DESC";

                    var parameters = new
                    {
                        MiscTypeCode = MiscEnumEntity.MaintenanceRequestType.MiscCode,
                        MiscCode = MiscEnumEntity.MaintenanceRequestTypeExternal.Code
                    };

                    var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
                    return result.ToList();
                }
                
                public async Task<bool> GetWOclosedAsync(int Id)
                {                      
                                var query = @"
                    SELECT COUNT(1)
                    FROM Maintenance.WorkOrder WO
                    INNER JOIN Maintenance.MiscMaster M ON M.Id = WO.StatusId
                    INNER JOIN Maintenance.MiscTypeMaster T ON T.Id = M.MiscTypeId
                    WHERE WO.RequestId = @RequestId
                    AND M.Code = @MiscCode
                    AND T.MiscTypeCode = @MiscTypeCode";
                   var parameters = new
                    {
                        RequestId = Id,
                        MiscTypeCode = MiscEnumEntity.WOStatus.MiscCode,
                        MiscCode = MiscEnumEntity.MaintenanceStatusUpdate.Code 
                    };
                    
                     var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                        return count > 0;
                } 


                 public async Task<bool> GetWOclosedOrInProgressAsync(int id)
                {                      
                                var query = @"
                    SELECT COUNT(1)
                    FROM   Maintenance.MaintenanceRequest R  
                    left JOIN Maintenance.WorkOrder WO  ON R.Id=WO.RequestId   
					left JOIN Maintenance.MiscMaster M ON M.Id = WO.StatusId
                    left JOIN Maintenance.MiscTypeMaster T ON T.Id = M.MiscTypeId
                    INNER JOIN Maintenance.MiscMaster MN ON MN.Id = R.RequestTypeId
					 INNER JOIN Maintenance.MiscMaster MJ ON MJ.Id = R.MaintenanceTypeId
                    WHERE R.Id = @RequestId AND M.Code = @MiscCode AND MJ.Code  = @Maintenancetype   AND MN.Code = @MiscCodeexternal                  
                    AND T.MiscTypeCode = @MiscTypeCode ";
                   var parameters = new
                    {
                        RequestId = id,
                        MiscTypeCode = MiscEnumEntity.WOStatus.MiscCode,
                        MiscCode= MiscEnumEntity.GetStatusId.Status ,
                       MiscCodeexternal = MiscEnumEntity.MaintenanceRequestTypeInternal.Code,
                       Maintenancetype =MiscEnumEntity.MaintenanceType.Code
                    };
                    
                     var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                        return count > 0;
                } 
                
                

                  public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceStatusDescAsync()
        {
            const string query = @"
                        SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                        WHERE (MiscTypeCode = @MiscTypeCode) 
                        AND  M.IsDeleted=0 and M.IsActive=1
                        ORDER BY SortOrder DESC";
            var parameters = new { MiscTypeCode = MiscEnumEntity.MaintenanceRequestType.MiscCode };
            var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
            return result.ToList();
        } 

                    public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceServiceDescAsync()
                    {
                    const string query = @"
                        SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                        WHERE (MiscTypeCode = @MiscTypeCode) 
                        AND  M.IsDeleted=0 and M.IsActive=1
                        ORDER BY SortOrder DESC";    
                        var parameters = new { MiscTypeCode = MiscEnumEntity.MaintenanceServiceType.MiscCode };        
                        var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query,parameters);
                        return result.ToList();
                    }    
                    public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceServiceLocationDescAsync()
                    {
                    const string query = @"
                        SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                        WHERE (MiscTypeCode = @MiscTypeCode) 
                        AND  M.IsDeleted=0 and M.IsActive=1
                        ORDER BY SortOrder DESC";    
                        var parameters = new { MiscTypeCode = MiscEnumEntity.MaintenanceServiceLocation.MiscCode };        
                        var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query,parameters);
                        return result.ToList();
                    }    

                    public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceSpareTypeDescAsync()
                    {
                    const string query = @"
                        SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                        WHERE (MiscTypeCode = @MiscTypeCode) 
                        AND  M.IsDeleted=0 and M.IsActive=1
                        ORDER BY SortOrder DESC";    
                        var parameters = new { MiscTypeCode = MiscEnumEntity.MaintenanceSpareType.MiscCode };        
                        var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query,parameters);
                        return result.ToList();
                    } 
                      
                    
                    public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceDispatchModeDescAsync()
                    {
                    const string query = @"
                        SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                        FROM Maintenance.MiscMaster M
                        INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                        WHERE (MiscTypeCode = @MiscTypeCode) 
                        AND  M.IsDeleted=0 and M.IsActive=1
                        ORDER BY SortOrder DESC";    
                        var parameters = new { MiscTypeCode = MiscEnumEntity.MaintenanceDispatchMode.MiscCode };        
                        var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query,parameters);
                        return result.ToList();
                    }


        public async Task<(string MachineName, int DepartmentId, int Id)?> GetMachineInfoAsync(int id)
            {
                const string query = @"
                    SELECT 
                        M.MachineName,
                        MG.DepartmentId,
                        M.Id
                    FROM Maintenance.MachineMaster M
                    INNER JOIN Maintenance.MachineGroup MG ON M.MachineGroupId = MG.Id
                    WHERE M.Id = @id
                    ORDER BY M.Id DESC;
                ";

                var result = await _dbConnection.QueryFirstOrDefaultAsync<(string MachineName, int DepartmentId, int Id)>(query, new { id });
                return result; // null if not found
            }
    }
}
