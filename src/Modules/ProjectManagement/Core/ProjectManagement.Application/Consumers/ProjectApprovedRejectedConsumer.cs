using Contracts.Commands.Project;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Domain.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.Consumers
{
    public class ProjectApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedProjectCommand>
    {
        private readonly IProjectMasterCommandRepository _projectMasterCommandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly ILogger<ProjectApprovedRejectedConsumer> _logger;

        public ProjectApprovedRejectedConsumer(
            IProjectMasterCommandRepository projectMasterCommandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            ILogger<ProjectApprovedRejectedConsumer> logger)
        {
            _projectMasterCommandRepository = projectMasterCommandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedProjectCommand> context)
        {
            var msg = context.Message;

            try
            {
                _logger.LogInformation(
                    "ProjectApprovedRejectedConsumer received. CorrelationId={CorrelationId}, ProjectId={ProjectId}, Status={Status}",
                    msg.CorrelationId, msg.ModuleTransactionId, msg.Status);

                if (!string.Equals(msg.ModuleTypeName, MiscEnumEntity.ProjectMaster, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Skipping: ModuleTypeName={ModuleTypeName} is not ProjectMaster", msg.ModuleTypeName);
                    return;
                }

                var status = msg.Status;

                if (status != MiscEnumEntity.Approved &&
                    status != MiscEnumEntity.Rejected &&
                    status != MiscEnumEntity.Pending)
                {
                    _logger.LogWarning("Skipping: Unsupported Status={Status} for ProjectId={ProjectId}", status, msg.ModuleTransactionId);
                    return;
                }

                var approved = await _miscMasterQueryRepository
                    .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);

                var rejected = await _miscMasterQueryRepository
                    .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                var pending = await _miscMasterQueryRepository
                    .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

                var finalStatusId =
                    status == MiscEnumEntity.Approved ? approved.Id :
                    status == MiscEnumEntity.Rejected ? rejected.Id :
                    pending.Id;

                var ok = await _projectMasterCommandRepository.UpdateProjectApprovalStatusAsync(
                    msg.ModuleTransactionId,
                    finalStatusId,
                    context.CancellationToken);

                _logger.LogInformation(
                    "ProjectMaster status update {Result}. ProjectId={ProjectId}, StatusId={StatusId}",
                    ok ? "SUCCEEDED" : "FAILED",
                    msg.ModuleTransactionId,
                    finalStatusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ProjectApprovedRejectedConsumer failed. CorrelationId={CorrelationId}",
                    msg.CorrelationId);
                throw;
            }
        }
    }
}
