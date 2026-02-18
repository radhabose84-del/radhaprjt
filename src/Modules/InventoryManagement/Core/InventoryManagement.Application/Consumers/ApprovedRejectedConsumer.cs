#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Commands.Inventory;
using Contracts.Dtos.Common;
using Contracts.Dtos.Purchase;
using Contracts.Events.Inventory;
using Contracts.Events.Purchase;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Common;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedInventoryCommand>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _imapper;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMrsEntryCommandRepository _mrsEntryCommandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApprovedRejectedConsumer(IMediator mediator, IMapper imapper, ILogger<ApprovedRejectedConsumer> logger, IMiscMasterQueryRepository miscMasterQueryRepository, IMrsEntryCommandRepository mrsEntryCommandRepository, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _imapper = imapper;
            _logger = logger;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mrsEntryCommandRepository = mrsEntryCommandRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedInventoryCommand> context)
        {
            var msg = context.Message;

            // ✅ Null-safe defaults
            var lineStatus = msg.LineStatus ?? new List<UpdateLineStatusDto>();
            var partyContacts = msg.PartyContacts ?? new List<PartyRefDto>();
            var dynamicFields = msg.DynamicFields ?? new List<JsonElement>();
            try
            {
                _logger.LogInformation("Purchase Consumer Approval Status Update: {@Message}", msg);

                // Helper: publish completion for saga
                async Task PublishCompletedAsync()
                {
                    await context.Publish(new ApprovedRejectedInventoryCompletedEvent
                    {
                        CorrelationId = msg.CorrelationId,
                        ModuleTransactionId = msg.ModuleTransactionId
                    });
                }
                // -----------------------------
                // MATERIAL REQUEST SLIP (MRS)
                // -----------------------------
                if (msg.ModuleTypeName == MiscEnumEntity.MaterialRequest)
                {
                    var status = msg.Status;
                    var rfqId = msg.ModuleTransactionId;

                    if (status == MiscEnumEntity.Approved || status == MiscEnumEntity.Rejected)
                    {
                        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
                        var rejected = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

                        var finalStatusId = status == MiscEnumEntity.Approved ? approved.Id : rejected.Id;
                        await _mrsEntryCommandRepository.UpdateMrsApproveAsync(rfqId, finalStatusId);
                    }

                    await PublishCompletedAsync();
                    return;
                }
                // ✅ If module not handled, still complete so saga doesn't hang
                await PublishCompletedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inventory Consumer Approval Status failed. {@Message}", msg);

                // ✅ Inform saga
                await context.Publish(new ApprovedRejectedFailedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    IndentId = msg.ModuleTransactionId,
                    Reason = $"Unhandled ModuleTypeName={msg.ModuleTypeName} and status={msg.Status} and Message={ex.Message}",
                    LineStatus = msg.LineStatus
                });
                throw;
            }
        }
    }
}