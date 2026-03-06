using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalStepDetails
{
    public class ApprovalStepDetailCommandRepository : IApprovalStepDetailCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        public ApprovalStepDetailCommandRepository(NotificationDbContext notificationDbContext)
        {
            _notificationDbContext = notificationDbContext;
        }
        public async Task<int> CreateAsync(ApprovalStepDetail approvalStepDetail)
        {
             _notificationDbContext.Entry(approvalStepDetail);
            await _notificationDbContext.ApprovalStepDetail.AddAsync(approvalStepDetail);
            await _notificationDbContext.SaveChangesAsync();

            return approvalStepDetail.Id;
        }

        public async Task<bool> DeleteAsync(int id, ApprovalStepDetail approvalStepDetail)
        {
             var ApprovalStepDelete = await _notificationDbContext.ApprovalStepDetail.FirstOrDefaultAsync(u => u.Id == id);
            if (ApprovalStepDelete != null)
            {
                ApprovalStepDelete.IsDeleted = approvalStepDetail.IsDeleted;
                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(ApprovalStepDetail approvalStepDetail)
        {
             var existingApprovalStep = await _notificationDbContext.ApprovalStepDetail
              .Include(cf => cf.ApprovalStepUnitMappings)
            // .Include(cf => cf.RuleSkipApproverMappings)
            .Include(cf => cf.ApprovalStepDepartmentMappings)
            .FirstOrDefaultAsync(u => u.Id == approvalStepDetail.Id);
            
            if (existingApprovalStep != null)
            {
                 _notificationDbContext.ApprovalStepUnitMapping.RemoveRange(existingApprovalStep.ApprovalStepUnitMappings);

            //    _notificationDbContext.RuleSkipApproverMapping.RemoveRange(existingApprovalStep.RuleSkipApproverMappings);
                _notificationDbContext.ApprovalStepDepartmentMapping.RemoveRange(existingApprovalStep.ApprovalStepDepartmentMappings);

                existingApprovalStep.WorkFlowTypeId = approvalStepDetail.WorkFlowTypeId;
                existingApprovalStep.StepOrder = approvalStepDetail.StepOrder;
                 existingApprovalStep.ApprovalStepId = approvalStepDetail.ApprovalStepId;
                existingApprovalStep.StopOnFirstMatch = approvalStepDetail.StopOnFirstMatch;
                existingApprovalStep.TargetTypeId = approvalStepDetail.TargetTypeId;
                existingApprovalStep.TargetValueId = approvalStepDetail.TargetValueId;
                existingApprovalStep.IsEdit = approvalStepDetail.IsEdit;
                // existingApprovalStep.ApprovalTypeId = approvalStepDetail.ApprovalTypeId;
                // existingApprovalStep.SLAHours = approvalStepDetail.SLAHours;
                // existingApprovalStep.ApprovalTypeId = approvalStepDetail.ApprovalTypeId;
                // existingApprovalStep.OnSLAAction = approvalStepDetail.OnSLAAction;
                existingApprovalStep.IsActive = approvalStepDetail.IsActive;
                
                if (approvalStepDetail.ApprovalStepUnitMappings?.Any() == true)
                   await _notificationDbContext.ApprovalStepUnitMapping.AddRangeAsync(approvalStepDetail.ApprovalStepUnitMappings);

            //    if (approvalStepDetail.RuleSkipApproverMappings?.Any() == true)
            //        await _notificationDbContext.RuleSkipApproverMapping.AddRangeAsync(approvalStepDetail.RuleSkipApproverMappings);

                if (approvalStepDetail.ApprovalStepDepartmentMappings?.Any() == true)
                   await _notificationDbContext.ApprovalStepDepartmentMapping.AddRangeAsync(approvalStepDetail.ApprovalStepDepartmentMappings);

                return await _notificationDbContext.SaveChangesAsync() > 0;
            }
            
            return false;
        }
    }
}