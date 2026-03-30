// ============================================================
// OLD CODE (MassTransit direct consumer — before centralized dispatcher)
// ============================================================
// using AutoMapper;
// using Contracts.Commands.Party;
// using Contracts.Commands.Workflow;
// using Contracts.Interfaces;
// using PartyManagement.Application.Common.Interfaces;
// using PartyManagement.Application.Common.Interfaces.IMiscMaster;
// using PartyManagement.Application.Common.Interfaces.IPartyMaster;
// using PartyManagement.Domain.Common;
// using PartyManagement.Domain.Entities;
// using MassTransit;
// using Contracts.Events.Users;
//
// namespace PartyManagement.Application.Consumers
// {
//     public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedPartyCommand>
//     {
//         private readonly IMapper _imapper;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
//         private readonly IPartyMasterQueryRepository _partyMasterQueryRepository;
//         private readonly IPartyActivityLogCommandRepository _ipartyActivityLogCommandRepository;
//         private IIPAddressService _ipAddressService;
//
//         public ApprovedRejectedConsumer(IMapper imapper, IEventPublisher eventPublisher, IMiscMasterQueryRepository miscMasterQueryRepository,
//             IPartyMasterCommandRepository partyMasterCommandRepository, IPartyActivityLogCommandRepository ipartyActivityLogCommandRepository,
//             IIPAddressService ipAddressService, IPartyMasterQueryRepository participantMasterCommandRepository)
//         {
//             _imapper = imapper;
//             _eventPublisher = eventPublisher;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _partyMasterCommandRepository = partyMasterCommandRepository;
//             _ipartyActivityLogCommandRepository = ipartyActivityLogCommandRepository;
//             _ipAddressService = ipAddressService;
//             _partyMasterQueryRepository = participantMasterCommandRepository;
//         }
//
//         public async Task Consume(ConsumeContext<UpdateApprovedRejectedPartyCommand> context)
//         {
//             try
//             {
//                 var msg = context.Message;
//                 var status = msg.Status;
//                 var partyIds = msg.ModuleTransactionId;
//                 var type = msg.ModuleTypeName;
//
//                 if (type == MiscEnumEntity.PartyDocumentImage.PartyMaster)
//                 {
//                     if (status == MiscEnumEntity.PartyDocumentImage.Approved ||
//                             status == MiscEnumEntity.PartyDocumentImage.Rejected)
//                     {
//                         var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
//                             MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
//                             MiscEnumEntity.PartyDocumentImage.Approved);
//
//                         var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
//                             MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
//                             MiscEnumEntity.PartyDocumentImage.Rejected);
//
//                         var finalStatusId = status == MiscEnumEntity.PartyDocumentImage.Approved
//                             ? statusApproved.Id
//                             : statusRejected.Id;
//
//                         var party = await _partyMasterCommandRepository.GetByIdAsync(partyIds);
//                         if (party != null)
//                         {
//                             var prevStatusId = party.StatusId;
//                             var prevPartyStatus = party.PartyStatus;
//                             party.PartyStatus = status;
//                             party.StatusId = finalStatusId;
//
//                             await _partyMasterCommandRepository.FinalizePartyStatus(party);
//
//                             var log = new PartyActivityLog
//                             {
//                                 PartyId = partyIds,
//                                 TableName = "PartyMaster",
//                                 ColumnName = "PartyStatus - StatusId",
//                                 OldValue = "Pending",
//                                 NewValue = party.PartyStatus,
//                                 ActionType = party.PartyStatus,
//                                 ChangedBy = _ipAddressService.GetUserId(),
//                                 ChangedByName = _ipAddressService.GetUserName(),
//                                 ChangedIp = _ipAddressService.GetSystemIPAddress(),
//                                 ChangedOn = DateTimeOffset.UtcNow
//                             };
//
//                             await _ipartyActivityLogCommandRepository.InsertAsync(log);
//
//                             // =========== Portal access + integration event ===========
//                             // if (status == MiscEnumEntity.PartyDocumentImage.Approved && party.IsPortalAccessEnabled == true)
//                             // {
//                             //     var partyWithContacts = await _partyMasterCommandRepository.GetByIdWithContactsAsync(party.Id) ?? party;
//                             //     var contact = GetPrimaryContact(partyWithContacts)
//                             //                 ?? await _partyMasterCommandRepository.GetPrimaryContactAsync(party.Id);
//                             //
//                             //     var (companyIds, unitIds) = await _partyMasterQueryRepository.GetCompanyUnitMapAsync(party.Id);
//                             //     if (unitIds.Count == 0 && party.UnitId > 0) unitIds = new List<int> { party.UnitId };
//                             //
//                             //     var partyTypeCodes = await _partyMasterQueryRepository.GetPartyTypeCodesAsync(party.Id);
//                             //
//                             //     var evt = new PartyApprovedIntegrationEvent { ... };
//                             //     await context.Publish(evt);
//                             // }
//                         }
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine(ex.Message);
//             }
//         }
//
//         private static PartyContact? GetPrimaryContact(PartyManagement.Domain.Entities.PartyMaster p) =>
//             p.PartyContactTypes?
//             .OrderByDescending(c => string.Equals(c.ContactBy, "Primary", StringComparison.OrdinalIgnoreCase))
//             .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) || !string.IsNullOrWhiteSpace(c.MobileNo))
//             .FirstOrDefault();
//     }
// }
// ============================================================
// NEW CODE (centralized dispatcher pattern)
// ============================================================

using Contracts.Commands.Party;
using Contracts.Events.Users;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace PartyManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedPartyCommand>
    {
        private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
        private readonly IPartyMasterQueryRepository _partyMasterQueryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IPartyActivityLogCommandRepository _partyActivityLogCommandRepository;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            IPartyMasterCommandRepository partyMasterCommandRepository,
            IPartyMasterQueryRepository partyMasterQueryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IPartyActivityLogCommandRepository partyActivityLogCommandRepository,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _partyMasterCommandRepository = partyMasterCommandRepository;
            _partyMasterQueryRepository = partyMasterQueryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _partyActivityLogCommandRepository = partyActivityLogCommandRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedPartyCommand> context)
        {
            var msg = context.Message;

            try
            {
                _logger.LogInformation("Party Consumer Approval Status Update: {@Message}", msg);

                var status = msg.Status;
                var partyId = msg.ModuleTransactionId;

                if (msg.ModuleTypeName != MiscEnumEntity.PartyDocumentImage.PartyMaster)
                    return;

                if (status != MiscEnumEntity.PartyDocumentImage.Approved &&
                    status != MiscEnumEntity.PartyDocumentImage.Rejected)
                    return;

                var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
                    MiscEnumEntity.PartyDocumentImage.Approved);

                var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
                    MiscEnumEntity.PartyDocumentImage.Rejected);

                var finalStatusId = status == MiscEnumEntity.PartyDocumentImage.Approved
                    ? statusApproved.Id
                    : statusRejected.Id;

                var party = await _partyMasterCommandRepository.GetByIdAsync(partyId);
                if (party == null)
                {
                    _logger.LogWarning("Party not found for Id={PartyId}", partyId);
                    return;
                }

                // Update status fields
                party.PartyStatus = status;
                party.StatusId = finalStatusId;

                await _partyMasterCommandRepository.FinalizePartyStatus(party);

                // Activity log
                var log = new PartyActivityLog
                {
                    PartyId = partyId,
                    TableName = "PartyMaster",
                    ColumnName = "PartyStatus - StatusId",
                    OldValue = "Pending",
                    NewValue = party.PartyStatus,
                    ActionType = party.PartyStatus,
                    ChangedBy = msg.ModifiedBy,
                    ChangedByName = msg.ModifiedByName ?? "Unknown",
                    ChangedIp = msg.ModifiedIP ?? "Unknown",
                    ChangedOn = DateTime.UtcNow
                };

                await _partyActivityLogCommandRepository.InsertAsync(log);

                // Portal access: if approved and IsPortalAccessEnabled, publish integration event to create user
                if (status == MiscEnumEntity.PartyDocumentImage.Approved && party.IsPortalAccessEnabled)
                {
                    var partyWithContacts = await _partyMasterCommandRepository.GetByIdWithContactsAsync(party.Id) ?? party;
                    var contact = GetPrimaryContact(partyWithContacts)
                                ?? await _partyMasterCommandRepository.GetPrimaryContactAsync(party.Id);

                    var (companyIds, unitIds) = await _partyMasterQueryRepository.GetCompanyUnitMapAsync(party.Id);
                    if (unitIds.Count == 0 && party.UnitId > 0) unitIds = new List<int> { party.UnitId }.AsReadOnly();

                    IReadOnlyList<string> partyTypeCodes = await _partyMasterQueryRepository.GetPartyTypeCodesAsync(party.Id);

                    var evt = new PartyApprovedIntegrationEvent
                    {
                        CorrelationId = Guid.NewGuid(),
                        PartyId = party.Id,
                        PartyCode = party.PartyCode ?? string.Empty,
                        PartyName = party.PartyName ?? string.Empty,
                        Email = contact?.EmailID?.Trim() ?? "",
                        Mobile = contact?.MobileNo?.Trim() ?? "",
                        IsPortalAccessEnabled = true,
                        CompanyIds = companyIds,
                        UnitIds = unitIds,
                        PartyTypeCodes = partyTypeCodes,
                        DefaultCompanyId = companyIds.Count > 0 ? companyIds[0] : null,
                        DefaultUnitId = unitIds.Count > 0 ? unitIds[0] : null,
                        CreatedBy = msg.ModifiedBy,
                        CreatedByName = msg.ModifiedByName ?? "Unknown",
                        CreatedIp = msg.ModifiedIP ?? "Unknown",
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.Publish(evt);
                    _logger.LogInformation("PartyApprovedIntegrationEvent published for PartyId={PartyId}", party.Id);
                }

                _logger.LogInformation(
                    "Party {PartyId} status updated to {Status}", partyId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Party Consumer Approval Status failed. {@Message}", msg);
                throw;
            }
        }

        private static PartyContact? GetPrimaryContact(PartyManagement.Domain.Entities.PartyMaster p) =>
            p.PartyContactTypes?
            .OrderByDescending(c => string.Equals(c.ContactBy, "Primary", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) || !string.IsNullOrWhiteSpace(c.MobileNo))
            .FirstOrDefault();
    }
}



