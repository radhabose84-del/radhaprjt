using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRules
{
    public class ApprovalRuleCommandRepository : IApprovalRuleCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        public ApprovalRuleCommandRepository(NotificationDbContext notificationDbContext)
        {
            _notificationDbContext = notificationDbContext;
        }
        public async Task<int> CreateAsync(ApprovalRule approvalRule)
        {
             _notificationDbContext.Entry(approvalRule);
            await _notificationDbContext.ApprovalRule.AddAsync(approvalRule);
            await _notificationDbContext.SaveChangesAsync();

            return approvalRule.Id;
        }

        public async Task<bool> DeleteAsync(int id, ApprovalRule approvalRule)
        {
            var ApprovalRuleDelete = await _notificationDbContext.ApprovalRule.FirstOrDefaultAsync(u => u.Id == id);
            if (ApprovalRuleDelete != null)
            {
                ApprovalRuleDelete.IsDeleted = approvalRule.IsDeleted;
                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(ApprovalRule approvalRule)
        {
             var existingApprovalRule = await _notificationDbContext.ApprovalRule
                 .Include(x => x.Conditions)
                     .ThenInclude(x => x.Field) // Field = ApprovalDataField (detail row), not master
                 .FirstOrDefaultAsync(u => u.Id == approvalRule.Id);
            
             if (existingApprovalRule == null) return false;
            
             await using var tx = await _notificationDbContext.Database.BeginTransactionAsync();
            
             // ---- Update scalars
             existingApprovalRule.ActionId = approvalRule.ActionId;
             existingApprovalRule.ApprovalStepDetailId = approvalRule.ApprovalStepDetailId;
             existingApprovalRule.EffectiveFrom = approvalRule.EffectiveFrom;
             existingApprovalRule.EffectiveTo   = approvalRule.EffectiveTo;
             existingApprovalRule.Priority = approvalRule.Priority;
             existingApprovalRule.IsActive = approvalRule.IsActive;
            
             // Safety
             existingApprovalRule.Conditions ??= new List<ApprovalRuleCondition>();
             var incoming = approvalRule.Conditions ?? new List<ApprovalRuleCondition>();
            
             // Index existing conditions by Datafield.Id (if present)
             var existingByDatafieldId = existingApprovalRule.Conditions
                 .Where(c => c.Field != null && c.Field.Id > 0)
                 .ToDictionary(c => c.Field!.Id, c => c);
            
             // Track which existing conditions were matched to incoming payload
             var matchedExistingConditionIds = new HashSet<int>();
            
             // Upsert each incoming condition
             foreach (var dto in incoming)
             {
                 var incomingDfId = dto.Field?.Id ?? 0;
            
                 if (incomingDfId > 0 && existingByDatafieldId.TryGetValue(incomingDfId, out var existingCond))
                 {
                     // ===== UPDATE existing condition & its datafield by Id =====
                     existingCond.RuleId   = existingApprovalRule.Id;
                     existingCond.OperatorId = dto.OperatorId;
                     existingCond.RightTypeId = dto.RightTypeId;
                     existingCond.RightValue = dto.RightValue;
                     existingCond.Aggregate = dto.Aggregate;
            
                     // Keep FK usage safe
                     existingCond.FieldId = incomingDfId;
            
                     // Update the existing ApprovalDataField entity
                     var df = existingCond.Field!;
                     df.FieldKey  = dto.Field!.FieldKey;
                     df.JsonPath  = dto.Field!.JsonPath;
                     df.ValueTypeId = dto.Field!.ValueTypeId;
                     df.ScopeId     = dto.Field!.ScopeId;
            
                     matchedExistingConditionIds.Add(existingCond.Id);
                 }
                 else
                 {
                     // ===== ADD new condition (and datafield if dto.Field is provided) =====
                     var newCond = new ApprovalRuleCondition
                     {
                         RuleId    = existingApprovalRule.Id,
                         OperatorId  = dto.OperatorId,
                         RightTypeId = dto.RightTypeId,
                         RightValue= dto.RightValue,
                         Aggregate = dto.Aggregate,
                         FieldId   = dto.FieldId // keep FK set directly
                     };
            
                     if (dto.Field != null)
                     {
                         if (dto.Field.Id > 0)
                         {
                             // Client says this datafield exists -> fetch and update it, then link
                             var existingDatafield = await _notificationDbContext.Set<ApprovalDataField>()
                                 .FirstOrDefaultAsync(d => d.Id == dto.Field.Id);
            
                             if (existingDatafield != null)
                             {
                                 existingDatafield.FieldKey  = dto.Field.FieldKey;
                                 existingDatafield.JsonPath  = dto.Field.JsonPath;
                                 existingDatafield.ValueTypeId = dto.Field.ValueTypeId;
                                 existingDatafield.ScopeId     = dto.Field.ScopeId;
            
                                 newCond.Field = existingDatafield; // link the tracked DF
                             }
                             else
                             {
                                 // Id given but not found: treat as new
                                 newCond.Field = new ApprovalDataField
                                 {
                                     FieldKey  = dto.Field.FieldKey,
                                     JsonPath  = dto.Field.JsonPath,
                                     ValueTypeId = dto.Field.ValueTypeId,
                                     ScopeId     = dto.Field.ScopeId
                                 };
                             }
                         }
                         else
                         {
                             // New datafield to be created
                             newCond.Field = new ApprovalDataField
                             {
                                 FieldKey  = dto.Field.FieldKey,
                                 JsonPath  = dto.Field.JsonPath,
                                 ValueTypeId = dto.Field.ValueTypeId,
                                 ScopeId     = dto.Field.ScopeId
                             };
                         }
                     }
            
                     existingApprovalRule.Conditions.Add(newCond);
                     // Note: new condition gets Id on SaveChanges
                 }
             }
            
             // ===== DELETE conditions that were not matched by incoming (mirror the payload) =====
             // Only remove previously existing ones (Id > 0) that weren't referenced via datafield id
             var toRemove = existingApprovalRule.Conditions
                 .Where(c => c.Id > 0 && !matchedExistingConditionIds.Contains(c.Id))
                 .Where(c =>
                 {
                     var dfId = c.Field?.Id ?? 0;
                     // If any incoming references this dfId, keep it
                     return !(dfId > 0 && incoming.Any(d => d.Field?.Id == dfId));
                 })
                 .ToList();
            
             if (toRemove.Count > 0)
                 _notificationDbContext.RemoveRange(toRemove);
            
             var changed = await _notificationDbContext.SaveChangesAsync() > 0;
             await tx.CommitAsync();
             return changed;
        }
    }
}