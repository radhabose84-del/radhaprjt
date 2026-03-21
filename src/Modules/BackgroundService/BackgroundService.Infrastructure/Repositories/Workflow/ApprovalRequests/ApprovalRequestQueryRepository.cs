using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Dto;
using Contracts.Interfaces;
// using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests
{
    public class ApprovalRequestQueryRepository : IApprovalRequestQuery, IApprovalRequestGrpcQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipaddressService;
         private readonly IMapper _mapper;
        private readonly ILogger<ApprovalRequestQueryRepository> _logger;
        public ApprovalRequestQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection, IIPAddressService ipaddressService,
        ILogger<ApprovalRequestQueryRepository> logger, IMapper mapper)
        {
            _dbConnection = dbConnection;
            _ipaddressService = ipaddressService;
            _logger = logger;
            _mapper = mapper;
        }



        public async Task<List<ApprovalRequest>> GetApprovalRequestByWorkFlowTypeAsync(string WorkFlowType)
        {
            const string query = @"
            SELECT AR.Id,AR.ModuleTransactionId,AR.ApprovalStepDetailId,AR.ApprovalRuleId,MM.Id,MM.Code FROM [AppData].[ApprovalRequest] AR
            INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = AR.StatusId
            WHERE MM.Code=@Status AND AR.WorkflowType=@WorkflowType";

            var parameters = new
            {
                Status = MiscEnumEntity.Pending,
                WorkflowType = WorkFlowType
            };

            var ApprovalRequest = await _dbConnection.QueryAsync<ApprovalRequest, Domain.Entities.Notification.MiscMaster, ApprovalRequest>(
                query,
                (approvalReq, status) =>
                {
                    approvalReq.Status = new Domain.Entities.Notification.MiscMaster
                    {
                        Code = status.Code
                    };

                    return approvalReq;
                },
                parameters,
                splitOn: "Id"
                );



            return ApprovalRequest.ToList();
        }

        public async Task<List<ApproverListItemDto>> GetApproverListByWorkFlowTypeAsync(
            string workFlowType,
            IEnumerable<int> moduleTransactionIds)
        {

         
            const string query = @"

                            WITH ranked AS (
                    SELECT
                        AR.Id ,
                        AR.ModuleTransactionId,
                        AR.ApproverBinding,
                        AR.ApproverValue,
                         ISNULL(ASD.IsEdit, 0) AS IsEdit,
                        MM.Id   AS StatusId,
                        MM.Code AS StatusCode,                       
                        ASD.StepOrder,
                        ROW_NUMBER() OVER (
                            PARTITION BY AR.ModuleTransactionId
                            ORDER BY ASD.StepOrder ASC, AR.Id ASC  -- 🔹 lowest StepOrder per transaction
                        ) AS rn
                    FROM [AppData].[ApprovalRequest] AR
                    INNER JOIN [AppData].[MiscMaster] MM 
                        ON MM.Id = AR.StatusId
                    INNER JOIN [AppData].[ApprovalStepDetail] ASD 
                        ON ASD.Id = AR.ApprovalStepDetailId
                    WHERE
                        MM.Code = @Status                 -- e.g. 'Pending'
                        AND AR.WorkflowType = @WorkflowType
                        AND AR.ModuleTransactionId IN @ModuleTransactionIds
                )
                SELECT
                    Id ,
                    ModuleTransactionId,
                    ApproverBinding,
                    ApproverValue,
                     IsEdit,
                    StepOrder,
                    StatusId   AS  Id    ,
                    StatusCode  AS Code
                   
               FROM ranked
                WHERE rn = 1;   -- ✅ only the earliest StepOrder per ModuleTransaction
                ";

            // const string query = @"
            // WITH ranked AS (
            //     SELECT
            //         AR.Id,
            //         AR.ModuleTransactionId,
            //         AR.ApproverBinding,
            //         AR.ApproverValue,
            //         MM.Id AS StatusId ,
            //         MM.Code AS StatusCode,
            //         ISNULL(ASD.IsEdit, 0) AS IsEdit,
            //         ROW_NUMBER() OVER (
            //             PARTITION BY AR.ModuleTransactionId
            //             ORDER BY AR.Id ASC
            //         ) AS rn
            //     FROM [AppData].[ApprovalRequest] AR
            //     INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = AR.StatusId
            //     INNER JOIN [AppData].[ApprovalStepDetail] ASD ON ASD.Id = AR.ApprovalStepDetailId
            //     WHERE
            //         MM.Code = @Status
            //         AND AR.WorkflowType = @WorkflowType
            //         AND AR.ModuleTransactionId IN @ModuleTransactionIds
            // )
            // SELECT
            //     Id,
            //     ModuleTransactionId,
            //     ApproverBinding,
            //     ApproverValue,
            //     StatusId as Id,
            //     StatusCode as Code,
            //     IsEdit
            // FROM ranked
            // WHERE rn = 1;";

            var parameters = new
            {
                WorkflowType = workFlowType,
                Status = MiscEnumEntity.Pending,
                ModuleTransactionIds = moduleTransactionIds
            };

            var result = await _dbConnection.QueryAsync<ApproverListItemDto, StatusDto, ApproverListItemDto>(
        query,
        (row, status) =>
        {
            row.Status = status?.Code ?? string.Empty;   // ✅ string assignment
            return row;
        },
        parameters,
        splitOn: "Id"
    );

            return result.ToList();
        }
        // public async Task<List<ApproverListItemDto>> GetApproverListByWorkFlowTypeAsync(
        //     string workFlowType,
        //     IEnumerable<int> moduleTransactionIds)
        // {
        //     const string query = @"
        //     WITH ranked AS (
        //         SELECT
        //             AR.Id               ,
        //             AR.ModuleTransactionId,
        //             AR.ApproverBinding,
        //             AR.ApproverValue,
        //             MM.Id AS StatusId ,
        //             MM.Code    AS StatusCode,
        //             ISNULL(ASD.IsEdit, 0) AS IsEdit,    
        //             ROW_NUMBER() OVER (
        //                 PARTITION BY AR.ModuleTransactionId
        //                 ORDER BY AR.Id ASC
        //             ) AS rn
        //         FROM [AppData].[ApprovalRequest] AR
        //         INNER JOIN [AppData].[MiscMaster] MM
        //             ON MM.Id = AR.StatusId
        //         INNER JOIN [AppData].[ApprovalStepDetail] ASD
        //             ON ASD.Id = AR.ApprovalStepDetailId
        //         WHERE
        //             MM.Code = @Status
        //             AND AR.WorkflowType = @WorkflowType
        //             AND AR.ModuleTransactionId IN @ModuleTransactionIds
        //     )
        //     SELECT
        //         Id,
        //         ModuleTransactionId,
        //         ApproverBinding,
        //         ApproverValue,
        //         StatusId as Id,
        //         StatusCode as Code,
        //         IsEdit
        //     FROM ranked
        //     WHERE rn = 1;";

        //     var parameters = new
        //     {
        //         WorkflowType = workFlowType,
        //         Status = MiscEnumEntity.Pending,
        //         ModuleTransactionIds = moduleTransactionIds
        //     };

        //     var result = await _dbConnection.QueryAsync<ApproverListItemDto>(query, parameters);
        //     return result.ToList();
        // }

        // public async Task<List<ApprovalRequest>> GetApproverListByWorkFlowTypeAsync(string WorkFlowType, IEnumerable<int> ModuleTransactionIds)
        // {
        //     const string query = @"
        //     WITH ranked AS (
        //         SELECT
        //             AR.Id,
        //             AR.ModuleTransactionId,
        //             AR.ApproverBinding,
        //             AR.ApproverValue,
        //             MM.Id AS StatusId ,         
        //             MM.Code  AS StatusCode, 
        //             ASD.IsEdit AS IsEdit ,   
        //             ROW_NUMBER() OVER (
        //                 PARTITION BY AR.ModuleTransactionId
        //                 ORDER BY AR.Id ASC 
        //             ) AS rn
        //         FROM [AppData].[ApprovalRequest]       AR
        //         INNER JOIN [AppData].[MiscMaster]      MM ON MM.Id = AR.StatusId
        //         INNER JOIN [AppData].[ApprovalStepDetail] ASD ON ASD.Id= AR.ApprovalStepDetailId
        //         WHERE
        //             MM.Code = @Status
        //             AND AR.WorkflowType = @WorkflowType AND AR.ModuleTransactionId IN @ModuleTransactionIds
        //     )
        //     SELECT
        //         Id,
        //         ModuleTransactionId,
        //         ApproverBinding,
        //         ApproverValue,
        //         StatusId as Id,
        //         StatusCode as Code,
        //         IsEdit
        //     FROM ranked
        //     WHERE rn = 1;";

        //     //  const string query = @"

        //     //     SELECT
        //     //        TOP 1 AR.Id,
        //     //         AR.ApproverBinding,
        //     //         AR.ApproverValue,
        //     //         MM.Id  ,         
        //     //         MM.Code 
        //     //     FROM [AppData].[ApprovalRequest]     AR
        //     //     INNER JOIN [AppData].[MiscMaster]      MM ON MM.Id = AR.StatusId
        //     //     WHERE
        //     //         MM.Code = @Status
        //     //         AND AR.WorkflowType = @WorkflowType AND AR.ModuleTransactionId IN @ModuleTransactionIds
        //     //         order by AR.Id
        //     // ";

        //     var parameters = new
        //     {
        //         WorkflowType = WorkFlowType,
        //         Status = MiscEnumEntity.Pending,
        //         ModuleTransactionIds
        //     };

        //     var ApprovalRequest = await _dbConnection.QueryAsync<ApprovalRequest, Domain.Entities.Notification.MiscMaster, ApprovalRequest>(
        //         query,
        //         (approvalReq, status) =>
        //         {
        //             approvalReq.Status = new Domain.Entities.Notification.MiscMaster
        //             {
        //                 Code = status.Code
        //             };

        //             return approvalReq;
        //         },
        //         parameters,
        //         splitOn: "Id"
        //         );



        //     return ApprovalRequest.ToList();
        // }


        public async Task<List<dynamic>> ApprovalRequestLineStatusByWorkFlowType(string WorkFlowType, IEnumerable<int> ModuleTransactionIds, int UserId)
        {
            const string query = @"
                        ;WITH LineRows AS (
                SELECT
                    ARL.ModuleLineTransactionId,
                    MM.Code AS ApproverStatusCode
                FROM [AppData].[ApprovalRequestLine] ARL
                INNER JOIN [AppData].[ApprovalRequest] AR ON AR.Id=ARL.ApprovalRequestId
                INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = ARL.StatusId
                WHERE AR.WorkflowType =@WorkflowType AND AR.ModuleTransactionId IN @ModuleTransactionIds AND AR.ApproverValue =@UserId

            ),
            LineRollup AS (
                SELECT
                    ModuleLineTransactionId,
                    SUM(CASE WHEN ApproverStatusCode = 'Rejected' THEN 1 ELSE 0 END) AS RejCount,
                    SUM(CASE WHEN ApproverStatusCode = 'Approved' THEN 1 ELSE 0 END) AS ApprCount,
                    COUNT(*)                                                          AS ApproverCount
                FROM LineRows
                GROUP BY ModuleLineTransactionId
            ),
            LineStatus AS (
                SELECT
                    ModuleLineTransactionId,
                    CASE
                        WHEN RejCount > 0 THEN 'Rejected'
                        WHEN ApproverCount > 0 AND ApprCount = ApproverCount THEN 'Approved'
                        ELSE 'Pending'
                    END AS Status
                FROM LineRollup
            )
            SELECT * FROM LineStatus
           
           ";



            var WorkflowType = await _dbConnection.QueryAsync<dynamic>
            (
            query,
            new
            {
                WorkFlowType,
                ModuleTransactionIds,
                UserId = UserId.ToString()
            });
            return WorkflowType.ToList();
        }
        public async Task<List<dynamic>> ApprovalRequestHeaderStatusByWorkFlowType(string WorkFlowType, IEnumerable<int> ModuleTransactionIds, int UserId)
        {
            const string query = @"
                       
                SELECT
                    ARL.ModuleLineTransactionId,
                    ARL.Id
                FROM [AppData].[ApprovalRequest] AR 
                INNER JOIN [AppData].[ApprovalRequestLine] ARL ON AR.Id=ARL.ApprovalRequestId
                WHERE AR.WorkflowType =@WorkflowType AND AR.ModuleTransactionId IN @ModuleTransactionIds AND AR.ApproverValue =@UserId

           
           ";

            var WorkflowType = await _dbConnection.QueryAsync<dynamic>
            (
            query,
            new
            {
                WorkFlowType,
                ModuleTransactionIds,
                UserId = UserId.ToString()
            });
            return WorkflowType.ToList();
        }

        public async Task<(List<dynamic> Lines, dynamic Header)> GetApprovalRequestById(int Id, int ModuleTransactionId)
        {



            const string query = @"
            Declare @WorkflowType varchar(100)
            SET @WorkflowType =(SELECT TOP 1 WorkflowType FROM [AppData].[ApprovalRequest] WHERE ModuleTransactionId=@ModuleTransactionId AND Id=@Id)
            ;WITH LineRows AS (
                SELECT
                    ARL.ModuleLineTransactionId,
                    MM.Code AS ApproverStatusCode
                FROM [AppData].[ApprovalRequestLine] ARL
                INNER JOIN [AppData].[ApprovalRequest] AR ON AR.Id=ARL.ApprovalRequestId
                INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = ARL.StatusId
                WHERE AR.ModuleTransactionId=@ModuleTransactionId AND AR.WorkflowType=@WorkflowType

            ),
            LineRollup AS (
                SELECT
                    ModuleLineTransactionId,
                    SUM(CASE WHEN ApproverStatusCode = @Rejected THEN 1 ELSE 0 END) AS RejCount,
                    SUM(CASE WHEN ApproverStatusCode = @Approved THEN 1 ELSE 0 END) AS ApprCount,
                    COUNT(*)                                                          AS ApproverCount
                FROM LineRows
                GROUP BY ModuleLineTransactionId
            ),
            LineStatus AS (
                SELECT
                    ModuleLineTransactionId,
                    CASE
                        WHEN RejCount > 0 THEN @Rejected
                        WHEN ApproverCount > 0 AND ApprCount = ApproverCount THEN @Approved
                        ELSE @Pending
                    END AS Status
                FROM LineRollup
            )

            SELECT ModuleLineTransactionId, Status FROM LineStatus;
			;WITH HeaderRows AS (
                SELECT
                    AR.ModuleTransactionId,
                    MM.Code AS ApproverStatusCode
                FROM [AppData].[ApprovalRequest] AR
                INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = AR.StatusId
                WHERE AR.ModuleTransactionId=@ModuleTransactionId AND AR.WorkflowType=@WorkflowType

            ),
            HeaderRollup AS (
                SELECT
                    ModuleTransactionId,
                    SUM(CASE WHEN ApproverStatusCode = @Rejected THEN 1 ELSE 0 END) AS RejCount,
                    SUM(CASE WHEN ApproverStatusCode = @Approved THEN 1 ELSE 0 END) AS ApprCount,
                    COUNT(*)                                                          AS ApproverCount
                FROM HeaderRows
                GROUP BY ModuleTransactionId
            ),
            HeaderStatus AS (
                SELECT
                    ModuleTransactionId,
                    CASE
                        WHEN RejCount > 0 THEN @Rejected
                        WHEN ApproverCount > 0 AND ApprCount = ApproverCount THEN @Approved
                        ELSE @Pending
                    END AS Status
                FROM HeaderRollup
            )
            
            SELECT ModuleTransactionId, Status AS StatusCode,@WorkflowType AS WorkflowType  FROM HeaderStatus;";

            var parameters = new
            {
                Pending = MiscEnumEntity.Pending,
                Rejected = MiscEnumEntity.Rejected,
                Approved = MiscEnumEntity.Approved,
                Id,
                ModuleTransactionId
            };

            if (_dbConnection.State != ConnectionState.Open)
            {
                await ((System.Data.Common.DbConnection)_dbConnection).OpenAsync();
            }

            using var tx = _dbConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {

                using var multi = await _dbConnection.QueryMultipleAsync(
                  sql: query,
                  param: parameters,
                  transaction: tx,
                  commandType: CommandType.Text,
                  commandTimeout: 60);

                var lines = (await multi.ReadAsync()).ToList();
                var header = await multi.ReadSingleOrDefaultAsync();


                tx.Commit();
                return new(lines, header);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                "Approval fetch failed. {Id} {ModuleTransactionId} Isolation={IsolationLevel}",
                Id, ModuleTransactionId, tx.IsolationLevel);
                try { tx.Rollback(); } catch { /* ignore */ }
                throw;
            }
        }

        public async Task<bool> IsApproveWorkflowConfigure(int MenuId, int UnitId, int DepartmentId)
        {
            var sql = @"
              SELECT CASE 
                       WHEN EXISTS (
                         SELECT 1 
                         FROM [AppData].[WorkflowType] W
                         INNER JOIN [AppData].[ApprovalStepDetail] ASD ON W.Id = ASD.WorkFlowTypeId
                         INNER JOIN [AppData].[ApprovalStepUnitMapping] ASM ON ASM.ApprovalStepDetailId = ASD.Id
                         LEFT JOIN [AppData].[ApprovalStepDepartmentMapping] D ON D.ApprovalStepDetailId = ASD.Id 
                         WHERE W.MenuId = @MenuId
                           AND UnitId = @UnitId
                           AND (DepartmentId = @DepartmentId OR DepartmentId IS NULL)
                       ) 
                       THEN CAST(1 AS bit) 
                       ELSE CAST(0 AS bit) 
               END";

            var result = await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                MenuId,
                UnitId,
                DepartmentId
            });

            return result;
        }

        public async Task<bool> IsLineLevelApproval(int ApprovalRequestId)
        {
            var sql = @"
              SELECT COUNT(*) FROM [AppData].[ApprovalRequest] AR
              INNER JOIN [AppData].[WorkflowType] WT ON WT.Id = AR.WorkflowTypeId
              WHERE AR.Id = @ApprovalRequestId AND WT.HasLine = 1";

            var result = await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                ApprovalRequestId
            });

            return result;
        }

        public async Task<dynamic> HeaderLevelApprovalStatus(int Id, int ModuleTransactionId)
        {
            const string query = @"
            Declare @WorkflowType varchar(100)
            SET @WorkflowType =(SELECT TOP 1 WorkflowType FROM [AppData].[ApprovalRequest] WHERE ModuleTransactionId=@ModuleTransactionId AND Id=@Id)
           
			;WITH HeaderRows AS (
                SELECT
                    AR.ModuleTransactionId,
                    MM.Code AS ApproverStatusCode
                FROM [AppData].[ApprovalRequest] AR
                INNER JOIN [AppData].[MiscMaster] MM ON MM.Id = AR.StatusId
                WHERE AR.ModuleTransactionId=@ModuleTransactionId AND AR.WorkflowType=@WorkflowType

            ),
            HeaderRollup AS (
                SELECT
                    ModuleTransactionId,
                    SUM(CASE WHEN ApproverStatusCode = @Rejected THEN 1 ELSE 0 END) AS RejCount,
                    SUM(CASE WHEN ApproverStatusCode = @Approved THEN 1 ELSE 0 END) AS ApprCount,
                    COUNT(*)                                                          AS ApproverCount
                FROM HeaderRows
                GROUP BY ModuleTransactionId
            ),
            HeaderStatus AS (
                SELECT
                    ModuleTransactionId,
                    CASE
                        WHEN RejCount > 0 THEN @Rejected
                        WHEN ApproverCount > 0 AND ApprCount = ApproverCount THEN @Approved
                        ELSE @Pending
                    END AS Status
                FROM HeaderRollup
            )
            
            SELECT ModuleTransactionId, Status AS StatusCode,@WorkflowType AS WorkflowType  FROM HeaderStatus;";

            var parameters = new
            {
                Pending = MiscEnumEntity.Pending,
                Rejected = MiscEnumEntity.Rejected,
                Approved = MiscEnumEntity.Approved,
                Id,
                ModuleTransactionId
            };

            var WorkflowType = await _dbConnection.QueryAsync<dynamic>
           (
           query,
           parameters);
            return WorkflowType.FirstOrDefault();
        }
        public async Task<List<ApprovalRequest>> GetApprovedHistory(string WorkflowType, int ModuleTransactionId)
        {
            var sql = @"
              SELECT AR.ApproverValue,AR.ModifiedDate,AR.RequestedDate,AR.Remark,MM.Code,ASD.StepOrder  FROM [AppData].[ApprovalRequest] AR
                INNER JOIN [AppData].[MiscMaster] MM ON AR.StatusId=MM.Id
                INNER JOIN [AppData].[ApprovalStepDetail] ASD ON ASD.Id=AR.ApprovalStepDetailId
                Where AR.ModuleTransactionId=@ModuleTransactionId AND AR.WorkflowType=@WorkflowType AND MM.Code=@Status";

            var parameters = new
            {
                ModuleTransactionId,
                WorkflowType,
                Status = MiscEnumEntity.Approved
            };

            var result = await _dbConnection.QueryAsync<ApprovalRequest, BackgroundService.Domain.Entities.Notification.MiscMaster, ApprovalStepDetail, ApprovalRequest>(
               sql,
               (approvalrequest, status, steporder) =>
               {
                   approvalrequest.Status = new BackgroundService.Domain.Entities.Notification.MiscMaster
                   {
                       Code = status.Code
                   };
                   approvalrequest.ApprovalStepDetail = new ApprovalStepDetail
                   {
                       StepOrder = steporder.StepOrder
                   };
                   return approvalrequest;
               },
                parameters,
                splitOn: "Code,StepOrder");

            return result.ToList();
        }
        public async Task<List<Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest.ApprovalRequestLineIdDto>> GetApprovalRequestLinesAsync(
            int approvalRequestHeaderId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    Id AS ApprovalRequestLineId,
                    ModuleLineTransactionId
                FROM [AppData].[ApprovalRequestLine]
                WHERE ApprovalRequestId = @HeaderId";

            var result = await _dbConnection.QueryAsync<Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest.ApprovalRequestLineIdDto>(
                new CommandDefinition(
                    sql,
                    new { HeaderId = approvalRequestHeaderId },
                    cancellationToken: cancellationToken));

            return result.ToList();
        }

         public async Task<List<Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById.ApprovalRequestWithLinesDto>> GetByModuleAsync(
        int moduleTransactionId,
        int workflowTypeId)
        {
            const string sql = @"
                SELECT 
                    ar.Id,
                    ar.WorkflowType,
                    ar.WorkflowTypeId,
                    ar.ModuleTransactionId,
                    ar.ApprovalStepDetailId,
                    ar.ApprovalRuleId,
                    ar.StatusId,
                    ar.RequestedDate,
                    ar.UnitId,
                    ar.DepartmentId,
                    ar.Remark,
                    ar.ModifiedBy,
                    ar.ModifiedDate,
                    ar.ModifiedByName,
                    ar.ModifiedIP,
                    ar.Action,
                    ar.ApproverBinding,
                    ar.ApproverValue,

                    l.Id,
                    l.ApprovalRequestId,
                    l.ModuleLineTransactionId,
                    l.StatusId,
                    l.Remark,
                    l.ModifiedBy,
                    l.ModifiedDate,
                    l.ModifiedByName,
                    l.ModifiedIP
                FROM AppData.ApprovalRequest ar
                LEFT JOIN AppData.ApprovalRequestLine l
                    ON l.ApprovalRequestId = ar.Id
                WHERE ar.ModuleTransactionId = @ModuleTransactionId
                AND ar.WorkflowTypeId      = @WorkflowTypeId
                ORDER BY ar.Id;";

            var lookup = new Dictionary<int, ApprovalRequest>();

            await _dbConnection.QueryAsync<ApprovalRequest, ApprovalRequestLine, ApprovalRequest>(
                sql,
                (header, line) =>
                {
                    if (!lookup.TryGetValue(header.Id, out var req))
                    {
                        req = header;
                        req.ApprovalRequestLines ??= new List<ApprovalRequestLine>();
                        lookup.Add(req.Id, req);
                    }

                    if (line != null && line.Id != 0)
                    {
                        req.ApprovalRequestLines.Add(line);
                    }

                    return req;
                },
                new
                {
                    ModuleTransactionId = moduleTransactionId,
                    WorkflowTypeId      = workflowTypeId
                },
                splitOn: "Id");

            var entities = lookup.Values.ToList();

            // 🔹 AutoMapper: entities -> DTOs
            var dtoList = _mapper.Map<List<Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById.ApprovalRequestWithLinesDto>>(entities);

            return dtoList;
        }

        // public async Task<List<ApprovalRequest>> GetByModuleAsync( int moduleTransactionId,  int workflowTypeId)
        // {
        //     const string sql = @"
        //         SELECT 
        //             ar.Id,
        //             ar.WorkflowType,
        //             ar.WorkflowTypeId,
        //             ar.ModuleTransactionId,
        //             ar.ApprovalStepDetailId,
        //             ar.ApprovalRuleId,
        //             ar.StatusId,
        //             ar.RequestedDate,
        //             ar.UnitId,
        //             ar.DepartmentId,
        //             ar.Remark,
        //             ar.ModifiedBy,
        //             ar.ModifiedDate,
        //             ar.ModifiedByName,
        //             ar.ModifiedIP,
        //             ar.Action,
        //             ar.ApproverBinding,
        //             ar.ApproverValue,

        //             -- Line columns (no aliases, match ApprovalRequestLine properties)
        //             l.Id,
        //             l.ApprovalRequestId,
        //             l.ModuleLineTransactionId,
        //             l.StatusId,
        //             l.Remark,
        //             l.ModifiedBy,
        //             l.ModifiedDate,
        //             l.ModifiedByName,
        //             l.ModifiedIP
        //         FROM AppData.ApprovalRequest ar
        //         LEFT JOIN AppData.ApprovalRequestLine l
        //             ON l.ApprovalRequestId = ar.Id
        //         WHERE ar.ModuleTransactionId = @ModuleTransactionId
        //         AND ar.WorkflowTypeId      = @WorkflowTypeId
        //         ORDER BY ar.Id;";

        //     var lookup = new Dictionary<int, ApprovalRequest>();

        //     await _dbConnection.QueryAsync<ApprovalRequest, ApprovalRequestLine, ApprovalRequest>(
        //         sql,
        //         (header, line) =>
        //         {
        //             if (!lookup.TryGetValue(header.Id, out var req))
        //             {
        //                 req = header;
        //                 // ✅ use the actual navigation property
        //                 req.ApprovalRequestLines ??= new List<ApprovalRequestLine>();
        //                 lookup.Add(req.Id, req);
        //             }

        //             if (line != null && line.Id != 0)
        //             {
        //                 req.ApprovalRequestLines.Add(line);
        //             }

        //             return req;
        //         },
        //         new
        //         {
        //             ModuleTransactionId = moduleTransactionId,
        //             WorkflowTypeId      = workflowTypeId
        //         },
        //         // We selected ar.Id first, then l.Id – so split on the second Id
        //         splitOn: "Id");

        //     return lookup.Values.ToList();
        // }

    }
}