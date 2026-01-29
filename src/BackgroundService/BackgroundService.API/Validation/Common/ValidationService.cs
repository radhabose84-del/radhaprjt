
using BackgroundService.API.Validation.NotificationConfig;

using BackgroundService.API.Validation.NotificationGroup;
using BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;

using BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup;
using FluentValidation;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using BackgroundService.API.Validation.NotificationTemplate;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using BackgroundService.API.Validation.NotificationGroupMember;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;

using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.API.Validation.Workflow.WorkflowTypes;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using BackgroundService.API.Validation.Workflow.ApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail;
using BackgroundService.API.Validation.Workflow.ApprovalRules;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Application.MiscMaster.Command.DeleteMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using BackgroundService.API.Validation.MiscTypeMaster;
using BackgroundService.Application.MiscMaster;
using BackgroundService.API.Validation.MiscMaster;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using BackgroundService.API.Validation.NotificationHierarchyAndEventRule;
using BackgroundService.API.Validation.Workflow.ApprovalRequest;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;

namespace BackgroundService.API.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreateNotificationConfigCommand>, CreateNotificationConfigCommandValidator>();
            services.AddScoped<IValidator<UpdateNotificationConfigCommand>, UpdateNotificationConfigCommandValidator>();
            services.AddScoped<IValidator<DeleteNotificationConfigCommand>, DeleteNotificationConfigCommandValidator>();

            services.AddScoped<IValidator<InsertNotificationHierarchyAndEventRuleCommand>, InsertNotificationHierarchyAndEventRuleCommandValidator>();
            services.AddScoped<IValidator<UpdateNotificationHierarchyAndEventRuleCommand>, UpdateNotificationHierarchyAndEventRuleCommandValidator>();
            services.AddScoped<IValidator<DeleteNotificationLevelHierarchyCommand>, DeleteNotificationLevelHierarchyCommandValidator>();

            services.AddScoped<IValidator<CreateNotificationGroupCommand>, CreateNotificationGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateNotificationGroupCommand>, UpdateNotificationGroupCommandValidator>();
            services.AddScoped<IValidator<DeleteNotificationGroupCommand>, DeleteNotificationGroupCommandValidator>();

            services.AddScoped<IValidator<CreateNotificationTemplateCommand>, CreateNotificationTemplateCommandValidator>();
            services.AddScoped<IValidator<UpdateNotificationTemplateCommand>, UpdateNotificationTemplateCommandValidator>();
            services.AddScoped<IValidator<DeleteNotificationTemplateCommand>, DeleteNotificationTemplateCommandValidator>();

            services.AddScoped<IValidator<CreateNotificationGroupMemberCommand>, CreateNotificationGroupMemberCommandValidator>();
            services.AddScoped<IValidator<UpdateNotificationGroupMemberCommand>, UpdateNotificationGroupMemberCommandValidator>();



            services.AddScoped<IValidator<CreateWorkflowTypeCommand>, CreateWorkflowTypeCommandValidator>();
            services.AddScoped<IValidator<UpdateWorkflowTypeCommand>, UpdateWorkflowTypeCommandValidator>();
            services.AddScoped<IValidator<DeleteWorkflowTypeCommand>, DeleteWorkflowTypeCommandValidator>();

            services.AddScoped<IValidator<CreateApprovalStepDetailCommand>, CreateApprovalStepDetailCommandValidator>();
            services.AddScoped<IValidator<UpdateApprovalStepDetailCommand>, UpdateApprovalStepDetailCommandValidator>();
            services.AddScoped<IValidator<DeleteApprovalStepDetailCommand>, DeleteApprovalStepDetailCommandValidator>();

            services.AddScoped<IValidator<CreateApprovalRuleCommand>, CreateApprovalRuleCommandValidator>();
            services.AddScoped<IValidator<UpdateApprovalRuleCommand>, UpdateApprovalRuleCommandValidator>();
            services.AddScoped<IValidator<DeleteApprovalRuleCommand>, DeleteApprovalRuleCommandValidator>();

            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<ApproveApprovalRequestCommand>, ApproveRejectCommandValidator>();
            

        }
    }
}