using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRules
{
    public class ApprovalRuleQueryRepository : IApprovalRuleQuery
    {
        private readonly IDbConnection _dbConnection;
        public ApprovalRuleQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<bool> AlreadyExistsAsync(string ConditionKey, string Operator, string Value, string Action, int UnitId, int WorkFlowTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) FROM [AppData].[ApprovalRule] WHERE ConditionKey = @ConditionKey
             
             AND Operator = @Operator AND Value = @Value AND Action = @Action AND UnitId = @UnitId AND WorkflowTypeId = @WorkflowTypeId AND IsDeleted = 0";
            var parameters = new DynamicParameters(new { ConditionKey, Operator, Value, Action, UnitId, WorkFlowTypeId });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

        public async Task<(List<ApprovalRule>, int)> GetAllApprovalRuleAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            const string dataQuery = @" SELECT 
                AR.Id, 
                AR.ActionId,
                AR.IsActive,
				AR.CreatedDate,
				AR.CreatedBy,
				AR.CreatedByName,
				AR.ModifiedBy,
				AR.ModifiedDate,
				AR.ModifiedByName,
                AR.EffectiveFrom,
                AR.EffectiveTo,
                AR.Priority,
                Action.Id,
                Action.Code,
                ASD.Id,
                ASD.WorkFlowTypeId,
                WT.Id,
                WT.MenuId
            FROM [AppData].[ApprovalRule] AR
            INNER JOIN [AppData].[MiscMaster] Action ON Action.Id = AR.ActionId
            INNER JOIN [AppData].[ApprovalStepDetail] ASD ON ASD.Id = AR.ApprovalStepDetailId
            INNER JOIN [AppData].[WorkflowType] WT ON WT.Id = ASD.WorkFlowTypeId
            WHERE 
            AR.IsDeleted = 0
                
                ORDER BY AR.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";
            const string countQuery = @"
              SELECT COUNT(*) 
               FROM [AppData].[ApprovalRule] AR
               INNER JOIN [AppData].[MiscMaster] Action ON Action.Id = AR.ActionId
               INNER JOIN [AppData].[ApprovalStepDetail] ASD ON ASD.Id = AR.ApprovalStepDetailId
               INNER JOIN [AppData].[WorkflowType] WT ON WT.Id = ASD.WorkFlowTypeId
             WHERE AR.IsDeleted = 0  ;
          ";


            var parameters = new
            {
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

              var rows = await _dbConnection.QueryAsync<
                  ApprovalRule,           
                  Domain.Entities.Notification.MiscMaster,             
                  ApprovalStepDetail,     
                  WorkflowType,           
                  ApprovalRule           
              >(
                  sql: dataQuery,
                  map: (ar, action, asd, wt) =>
                  {
                      asd.WorkflowType = wt;      
                      ar.Action = action;        
                      ar.ApprovalStepDetail = asd;
                      return ar;
                  },
                  param: parameters,
                  splitOn: "Id,Id,Id"
              );

              var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

              return (rows.ToList(), totalCount);
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [AppData].[ApprovalRule]  WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }
        
        
         public async Task<ApprovalRule> GetByIdAsync(int id)
        {
            const string sql = @"
            SELECT
               
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
                ARC.RightValue       ,
                ARC.Aggregate        ,

                -- DATA FIELD
                ADF.Id                ,
                ADF.FieldKey          ,
                ADF.JsonPath          ,
                ADF.ValueTypeId         ,
                ADF.ScopeId            
            FROM  [AppData].[ApprovalRule] AR
                
            LEFT JOIN  [AppData].[ApprovalRuleCondition] ARC
                ON ARC.RuleId = AR.Id
            LEFT JOIN  [AppData].[ApprovalDataField] ADF
                ON ADF.Id = ARC.FieldId
            WHERE AR.IsDeleted = 0
              AND AR.IsActive = 1
              AND AR.Id = @Id;
            ";

             
             var ruleLookup = new Dictionary<int, ApprovalRule>();

             // Multi-mapping across 6 types; use splitOn to tell Dapper where each object starts
             var _ = await _dbConnection.QueryAsync<
                 ApprovalRule,
                 ApprovalRuleCondition,
                 ApprovalDataField,
                 ApprovalRule>(
                 sql,
                 ( ar, arc, adf) =>
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
                                         FieldKey = adf.FieldKey,
                                         JsonPath = adf.JsonPath,
                                         ValueTypeId = adf.ValueTypeId,
                                         ScopeId = adf.ScopeId
                                     }
                                     : null
                             };

                             rule.Conditions.Add(condition);
                         }
                     

                     return rule;
                 },
                 new { Id = id },
                 splitOn: "Id,Id"
             );

             // Single step is expected for the given Id
             return ruleLookup.Values.FirstOrDefault();
         
        }
    }
}