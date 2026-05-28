using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalStepDetails
{
    public class ApprovalStepDetailQueryRepository : IApprovalStepDetailQuery
    {
        private readonly IDbConnection _dbConnection;
        public ApprovalStepDetailQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<bool> AlreadyExistsAsync(int WorkFlowTypeId, int TargetTypeId, int ApprovalStepId, int ApprovalTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) FROM [AppData].[ApprovalStepDetail] WHERE WorkFlowTypeId = @WorkFlowTypeId
             
             AND TargetTypeId = @TargetTypeId AND ApprovalStepId = @ApprovalStepId AND ApprovalTypeId = @ApprovalTypeId AND IsDeleted = 0";
            var parameters = new DynamicParameters(new { WorkFlowTypeId, TargetTypeId, ApprovalStepId, ApprovalTypeId });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

        public async Task<(List<ApprovalStepDetail>, int)> GetAllApprovalStepDetailAsync(int PageNumber, int PageSize, string? SearchTerm)
        {


            const string dataQuery = @" SELECT 
                ASD.Id, 
                ASD.WorkFlowTypeId,
                ASD.StepOrder,
                ASD.TargetTypeId,
                ASD.StepOrder,
                ASD.TargetValueId,
                ASD.StopOnFirstMatch,
                ASD.IsEdit,
                ASD.IsActive,ASD.CreatedDate,ASD.CreatedBy,ASD.CreatedByName,ASD.ModifiedBy,ASD.ModifiedDate,ASD.ModifiedByName,
                ApprovalStep.Id,ApprovalStep.Code,WorkFlow.Id,WorkFlow.MenuId,WorkFlow.TransactionTypeId,TargetType.Id,TargetType.Code
            FROM [AppData].[ApprovalStepDetail] ASD
            INNER JOIN [AppData].[MiscMaster] ApprovalStep on ApprovalStep.Id=ASD.ApprovalStepId
            INNER JOIN [AppData].[WorkflowType] WorkFlow on WorkFlow.Id=ASD.WorkFlowTypeId
            INNER JOIN [AppData].[MiscMaster] TargetType on TargetType.Id=ASD.TargetTypeId
            WHERE 
            ASD.IsDeleted = 0
                AND (@Search IS NULL OR ApprovalStep.Code LIKE @Search )
                ORDER BY ASD.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";
            const string countQuery = @"
              SELECT COUNT(*) 
               FROM [AppData].[ApprovalStepDetail] ASD
            INNER JOIN [AppData].[MiscMaster] ApprovalStep on ApprovalStep.Id=ASD.ApprovalStepId
            INNER JOIN [AppData].[WorkflowType] WorkFlow on WorkFlow.Id=ASD.WorkFlowTypeId
            INNER JOIN [AppData].[MiscMaster] TargetType on TargetType.Id=ASD.TargetTypeId
             WHERE ASD.IsDeleted = 0  AND (@Search IS NULL OR ApprovalStep.Code LIKE @Search );
          ";


            var parameters = new
            {
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var ApprovalStep = await _dbConnection.QueryAsync<ApprovalStepDetail, Domain.Entities.Notification.MiscMaster, WorkflowType,Domain.Entities.Notification.MiscMaster, ApprovalStepDetail>(
                dataQuery,
                (detail, approvalStep, workFlow, targetType) =>
                {
                    detail.ApprovalStep = new Domain.Entities.Notification.MiscMaster
                    {
                        Id = approvalStep.Id,
                        Code = approvalStep.Code
                    };
                    detail.WorkflowType = new WorkflowType
                    {
                        Id = workFlow.Id,
                        MenuId = workFlow.MenuId,
                        TransactionTypeId = workFlow.TransactionTypeId
                    };
                     detail.TargetType = new Domain.Entities.Notification.MiscMaster
                    {
                        Id = targetType.Id,
                        Code = targetType.Code
                    };
                    //  detail.ApprovalType = new Domain.Entities.Notification.MiscMaster
                    //  {
                    //      Id = approvalType.Id,
                    //      Code = approvalType.Code
                    //  };
                    return detail;
                },
                parameters,
                splitOn: "Id,Id,Id,Id"
                );

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

            return (ApprovalStep.ToList(), totalCount);
        }

        public async Task<ApprovalStepDetail> GetByIdAsync(int id)
        {
            const string sql = @"
            SELECT
                -- STEP
                ASD.Id,
                ASD.WorkFlowTypeId,
                ASD.StepOrder,
                ASD.TargetTypeId,
                ASD.TargetValueId,
                ASD.ApprovalStepId,
                ASD.StopOnFirstMatch,
                ASD.IsEdit,
                ASD.IsActive,

                -- UNIT MAP
                ASM.Id                  ,
                ASM.UnitId              ,
                ASM.ApprovalStepDetailId ,

                -- DEPT MAP
                ApprovalDept.Id                         ,
                ApprovalDept.DepartmentId               ,
                ApprovalDept.ApprovalStepDetailId       ,

                -- RULE
                AR.Id                 ,
                AR.ActionId             ,
                AR.ApprovalStepDetailId ,
                AR.EffectiveFrom      ,
                AR.EffectiveTo        ,
                AR.Priority           ,

                -- RULE CONDITION
                ARC.Id                ,
                ARC.RuleId            ,
                ARC.FieldId           ,
                ARC.OperatorId          ,
                ARC.RightTypeId         ,
                ARC.RightValue        ,
                ARC.Aggregate        ,

                -- DATA FIELD
                ADF.Id                ,
                ADF.JsonPath          ,
                ADF.ValueTypeId         ,
                ADF.ScopeId            
            FROM [AppData].[ApprovalStepDetail] ASD
            INNER JOIN [AppData].[ApprovalStepUnitMapping] ASM
                ON ASM.ApprovalStepDetailId = ASD.Id
            LEFT JOIN [AppData].[ApprovalStepDepartmentMapping] ApprovalDept
                ON ApprovalDept.ApprovalStepDetailId = ASD.Id
            LEFT JOIN  [AppData].[ApprovalRule] AR
                ON AR.ApprovalStepDetailId = ASD.Id
            LEFT JOIN  [AppData].[ApprovalRuleCondition] ARC
                ON ARC.RuleId = AR.Id
            LEFT JOIN  [AppData].[ApprovalDataField] ADF
                ON ADF.Id = ARC.FieldId
            WHERE ASD.IsDeleted = 0
              AND ASD.IsActive = 1
              AND ASD.Id = @Id;
            ";

            // Ensure connection is open
            //  if (_dbConnection.State != System.Data.ConnectionState.Open)
            //      await _dbConnection.OpenAsync();

            var stepLookup = new Dictionary<int, ApprovalStepDetail>();
            var ruleLookup = new Dictionary<int, ApprovalRule>();

            // Multi-mapping across 6 types; use splitOn to tell Dapper where each object starts
            var _ = await _dbConnection.QueryAsync<
                ApprovalStepDetail,
                ApprovalStepUnitMapping,
                ApprovalStepDepartmentMapping,
                ApprovalRule,
                ApprovalRuleCondition,
                ApprovalDataField,
                ApprovalStepDetail>(
                sql,
                (asd, asm, dept, ar, arc, adf) =>
                {
                    // Root: ApprovalStepDetail
                    if (!stepLookup.TryGetValue(asd.Id, out var step))
                    {
                        step = asd;
                        step.ApprovalStepUnitMappings = step.ApprovalStepUnitMappings ?? new List<ApprovalStepUnitMapping>();
                        step.ApprovalStepDepartmentMappings = step.ApprovalStepDepartmentMappings ?? new List<ApprovalStepDepartmentMapping>();
                        step.ApprovalRules = step.ApprovalRules ?? new List<ApprovalRule>();
                        stepLookup[step.Id] = step;
                    }

                    // Units (dedupe)
                    if (asm != null && asm.Id != 0 && !step.ApprovalStepUnitMappings.Any(u => u.Id == asm.Id))
                    {
                        step.ApprovalStepUnitMappings.Add(new ApprovalStepUnitMapping
                        {
                            Id = asm.Id,
                            UnitId = asm.UnitId,
                            ApprovalStepDetailId = asm.ApprovalStepDetailId
                        });
                    }

                    // Departments (dedupe)
                    if (dept != null && dept.Id != 0 && !step.ApprovalStepDepartmentMappings.Any(d => d.Id == dept.Id))
                    {
                        step.ApprovalStepDepartmentMappings.Add(new ApprovalStepDepartmentMapping
                        {
                            Id = dept.Id,
                            DepartmentId = dept.DepartmentId,
                            ApprovalStepDetailId = dept.ApprovalStepDetailId
                        });
                    }

                    // Rules and nested Conditions
                    if (ar != null && ar.Id != 0)
                    {
                        if (!ruleLookup.TryGetValue(ar.Id, out var rule))
                        {
                            rule = new ApprovalRule
                            {
                                Id = ar.Id,
                                ActionId = ar.ActionId,
                                ApprovalStepDetailId = ar.ApprovalStepDetailId,
                                EffectiveFrom = ar.EffectiveFrom,
                                EffectiveTo = ar.EffectiveTo,
                                Priority = ar.Priority,
                                Conditions = new List<ApprovalRuleCondition>()
                            };
                            ruleLookup[rule.Id] = rule;
                            step.ApprovalRules.Add(rule);
                        }

                        // Condition + DataField (one-to-one)
                        if (arc != null && arc.Id != 0 && !rule.Conditions.Any(c => c.Id == arc.Id))
                        {
                            var condition = new ApprovalRuleCondition
                            {
                                Id = arc.Id,
                                RuleId = arc.RuleId,
                                FieldId = arc.FieldId,
                                OperatorId = arc.OperatorId,
                                RightTypeId = arc.RightTypeId,
                                RightValue = arc.RightValue,
                                Aggregate = arc.Aggregate,
                                Field = (adf != null && adf.Id != 0)
                                    ? new ApprovalDataField
                                    {
                                        Id = adf.Id,
                                        JsonPath = adf.JsonPath,
                                        ValueTypeId = adf.ValueTypeId,
                                        ScopeId = adf.ScopeId
                                    }
                                    : null
                            };

                            rule.Conditions.Add(condition);
                        }
                    }

                    return step;
                },
                new { Id = id },
                splitOn: "Id,Id,Id,Id,Id"
            );

            // Single step is expected for the given Id
            return stepLookup.Values.FirstOrDefault();

        }

        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [AppData].[ApprovalStepDetail]  WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }
          public async Task<List<ApprovalStepDetail>> GetApprovalStepDetailAutoComplete(string searchPattern)
        {
            const string dataQuery = @" SELECT 
                ASD.Id, 
                ASD.TargetTypeId,
                ASD.StepOrder,
                ASD.TargetValueId,ASD.ISEdit,
                ApprovalStep.Id,ApprovalStep.Code,WorkFlow.Id,WorkFlow.MenuId
            FROM [AppData].[ApprovalStepDetail] ASD
            INNER JOIN [AppData].[MiscMaster] ApprovalStep on ApprovalStep.Id=ASD.ApprovalStepId
            INNER JOIN [AppData].[WorkflowType] WorkFlow on WorkFlow.Id=ASD.WorkFlowTypeId
            WHERE 
            ASD.IsDeleted = 0
                AND (@Search IS NULL OR ApprovalStep.Code LIKE @Search)
                ORDER BY ASD.Id desc;
            ";
         


            var parameters = new
            {
                Search = string.IsNullOrEmpty(searchPattern) ? null : $"%{searchPattern}%"
            };

            var ApprovalStep = await _dbConnection.QueryAsync<ApprovalStepDetail, Domain.Entities.Notification.MiscMaster, WorkflowType, ApprovalStepDetail>(
                dataQuery,
                (detail, approvalStep, workFlow) =>
                {
                    detail.ApprovalStep = new Domain.Entities.Notification.MiscMaster
                    {
                        Id = approvalStep.Id,
                        Code = approvalStep.Code
                    };
                    detail.WorkflowType = new WorkflowType
                    {
                        Id = workFlow.Id,
                        MenuId = workFlow.MenuId
                    };
                    
                    return detail;
                },
                parameters,
                splitOn: "Id,Id"
                );

            

            return ApprovalStep.ToList();
        }
    }
}