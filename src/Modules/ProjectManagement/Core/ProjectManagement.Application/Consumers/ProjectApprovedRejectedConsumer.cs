// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Commands.Project;
// using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
// using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
// using ProjectManagement.Domain.Common;
// using MassTransit;
// using Microsoft.Extensions.Logging;

// namespace ProjectManagement.Application.Consumers
// {
//     public class ProjectApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedProjectCommand>
//     {
//         private readonly IProjectMasterCommandRepository _projectMasterCommandRepository;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly ILogger<ProjectApprovedRejectedConsumer> _logger;

//         public ProjectApprovedRejectedConsumer(IProjectMasterCommandRepository projectMasterCommandRepository, IMiscMasterQueryRepository miscMasterQueryRepository, ILogger<ProjectApprovedRejectedConsumer> logger)
//         {
//             _projectMasterCommandRepository = projectMasterCommandRepository;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _logger = logger;
//         }

//         public async Task Consume(ConsumeContext<UpdateApprovedRejectedProjectCommand> context)
//         {
//             try
//             {
//                 var msg = context.Message;

//                 _logger.LogInformation(
//                     "ProjectApprovedRejectedConsumer received. CorrelationId={CorrelationId}, ProjectId={ProjectId}, ModuleTypeName={ModuleTypeName}, Status={Status}",
//                     msg.CorrelationId, msg.ModuleTransactionId, msg.ModuleTypeName, msg.Status);

//                 // If queue is shared for multiple modules, keep filter
//                 if (!string.Equals(msg.ModuleTypeName, MiscEnumEntity.ProjectMaster, StringComparison.OrdinalIgnoreCase))
//                 {
//                     _logger.LogDebug(
//                         "Skipping: ModuleTypeName={ModuleTypeName} is not ProjectMaster",
//                         msg.ModuleTypeName);
//                     return;
//                 }

//                 var status = msg.Status;

//                 // Only act on these statuses
//                 if (status != MiscEnumEntity.Approved &&
//                     status != MiscEnumEntity.Rejected &&
//                     status != MiscEnumEntity.Pending)
//                 {
//                     _logger.LogWarning(
//                         "Skipping: Unsupported Status={Status} for ProjectId={ProjectId}",
//                         status, msg.ModuleTransactionId);
//                     return;
//                 }

//                 // NOTE:
//                 // If your ProjectMaster uses MiscEnumEntity.Status instead of ApprovalStatus, swap the first param below.
//                 var approved = await _miscMasterQueryRepository
//                     .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);

//                 var rejected = await _miscMasterQueryRepository
//                     .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

//                 var pending = await _miscMasterQueryRepository
//                     .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

//                 var finalStatusId =
//                     status == MiscEnumEntity.Approved ? approved.Id :
//                     status == MiscEnumEntity.Rejected ? rejected.Id :
//                     pending.Id;

//                 var ok = await _projectMasterCommandRepository.UpdateProjectApprovalStatusAsync(
//                     msg.ModuleTransactionId,
//                     finalStatusId,
//                     context.CancellationToken);

//                 _logger.LogInformation(
//                     "ProjectMaster status update {Result}. ProjectId={ProjectId}, StatusId={StatusId}",
//                     ok ? "SUCCEEDED" : "FAILED",
//                     msg.ModuleTransactionId,
//                     finalStatusId);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex,
//                     "ProjectApprovedRejectedConsumer failed. CorrelationId={CorrelationId}, Message={@Message}",
//                     context.Message?.CorrelationId,
//                     context.Message);
//                 throw;
//             }
//         }
        

     
//     }
// }