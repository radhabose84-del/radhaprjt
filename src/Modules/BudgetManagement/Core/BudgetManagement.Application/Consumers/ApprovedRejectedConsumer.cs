using AutoMapper;
using Contracts.Commands.Budget;
using Contracts.Events.Workflow;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Domain.Common;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using BudgetManagement.Application.BudgetAllocation.Command.Update;
using Contracts.Events.Purchase;

namespace BudgetManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedBudgetCommand>
    {
        private readonly IBudgetRequestCommandRepository _requestCommand;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;
        private readonly IMediator _mediator;

        public ApprovedRejectedConsumer(
            IBudgetRequestCommandRepository requestCommand,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            ILogger<ApprovedRejectedConsumer> logger,
            IMediator mediator)
        {
            _requestCommand = requestCommand;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedBudgetCommand> context)
        {
            var msg = context.Message;

            async Task PublishCompletedAsync()
            {
                await context.Publish(new ApprovedRejectedBudgetCompletedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    ModuleTransactionId = msg.ModuleTransactionId
                });
            }

            try
            {
                _logger.LogInformation("Budget Consumer Approval Status Update: {@Message}", msg);

                // only handle BudgetRequest
                if (!string.Equals(msg.ModuleTypeName, MiscEnumEntity.BudgetRequest, StringComparison.OrdinalIgnoreCase))
                {
                    await PublishCompletedAsync(); // don’t hang saga
                    return;
                }

                if (msg.ModuleTransactionId <= 0)
                {
                    _logger.LogWarning("Invalid ModuleTransactionId: {Id}", msg.ModuleTransactionId);
                    await PublishCompletedAsync();
                    return;
                }

                var status = msg.Status;

                // Pending or unknown -> no update, but mark done so saga continues
                if (!string.Equals(status, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(status, MiscEnumEntity.Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    await PublishCompletedAsync();
                    return;
                }

                // resolve status id
                var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                var finalStatusId = string.Equals(status, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase)
                    ? approved.Id
                    : rejected.Id;
              
                var updated = await _requestCommand.UpdateRequestApproveAsync(msg.ModuleTransactionId, finalStatusId, context.CancellationToken);

                if (!updated)
                    throw new InvalidOperationException($"BudgetRequest approve update failed. BudgetRequestId={msg.ModuleTransactionId}");

                // If approved -> upsert allocation
                if (string.Equals(status, MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase))
                {
                    await _mediator.Send(new UpsertBudgetAllocationOnApprovalCommand
                    {
                        BudgetRequestId = msg.ModuleTransactionId
                    }, context.CancellationToken);
                }

                await PublishCompletedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Budget Consumer approval/rejection failed. {@Message}", msg);

                await context.Publish(new ApprovedRejectedBudgetFailedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    ModuleTransactionId = msg.ModuleTransactionId,
                    Reason = ex.Message
                });

                throw;
            }
        }
    }
}
