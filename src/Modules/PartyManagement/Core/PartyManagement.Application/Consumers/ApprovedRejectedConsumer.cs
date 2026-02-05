using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Commands.Party;
using Contracts.Commands.Workflow;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using MassTransit;
using Contracts.Events.Users;


namespace PartyManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedPartyCommand>
    {
        private readonly IMapper _imapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
        private readonly IPartyMasterQueryRepository _partyMasterQueryRepository;
        private readonly IPartyActivityLogCommandRepository _ipartyActivityLogCommandRepository;
        private IIPAddressService _ipAddressService;


        public ApprovedRejectedConsumer(IMapper imapper, IEventPublisher eventPublisher, IMiscMasterQueryRepository miscMasterQueryRepository,
            IPartyMasterCommandRepository partyMasterCommandRepository, IPartyActivityLogCommandRepository ipartyActivityLogCommandRepository,
            IIPAddressService ipAddressService, IPartyMasterQueryRepository participantMasterCommandRepository)
        {
            _imapper = imapper;
            _eventPublisher = eventPublisher;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _partyMasterCommandRepository = partyMasterCommandRepository;
            _ipartyActivityLogCommandRepository = ipartyActivityLogCommandRepository;
            _ipAddressService = ipAddressService;
            _partyMasterQueryRepository = participantMasterCommandRepository;

        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedPartyCommand> context)
        {

            try
            {
                var msg = context.Message;
                var status = msg.Status;
                var partyIds = msg.ModuleTransactionId;
                var type = msg.ModuleTypeName;

                if (type == MiscEnumEntity.PartyDocumentImage.PartyMaster)
                {
                    if (status == MiscEnumEntity.PartyDocumentImage.Approved ||
                            status == MiscEnumEntity.PartyDocumentImage.Rejected)
                    {
                        var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
                            MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
                            MiscEnumEntity.PartyDocumentImage.Approved);

                        var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
                            MiscEnumEntity.PartyDocumentImage.ApprovalStatus,
                            MiscEnumEntity.PartyDocumentImage.Rejected);

                        // Decide final StatusId once
                        var finalStatusId = status == MiscEnumEntity.PartyDocumentImage.Approved
                            ? statusApproved.Id
                            : statusRejected.Id;

                        var party = await _partyMasterCommandRepository.GetByIdAsync(partyIds);
                        if (party != null)
                        {

                            var prevStatusId = party.StatusId;
                            var prevPartyStatus = party.PartyStatus;
                            // Update only status-related fields
                            party.PartyStatus = status;
                            party.StatusId = finalStatusId;

                            await _partyMasterCommandRepository.FinalizePartyStatus(party);



                            var log = new PartyActivityLog
                            {
                                PartyId = partyIds,
                                TableName = "PartyMaster",
                                ColumnName = "PartyStatus - StatusId",
                                OldValue = "Pending",
                                NewValue = party.PartyStatus,
                                ActionType = party.PartyStatus,
                                ChangedBy = _ipAddressService.GetUserId(),
                                ChangedByName = _ipAddressService.GetUserName(),
                                ChangedIp = _ipAddressService.GetSystemIPAddress(),
                                ChangedOn = DateTimeOffset.UtcNow
                            };

                            await _ipartyActivityLogCommandRepository.InsertAsync(log);

                            // =========== publish only on transition TO Approved ===========
                            // Only publish on Approved (you can also check prevStatusId != approved.Id if you want true transition-only)
                            if (status == MiscEnumEntity.PartyDocumentImage.Approved  && party.IsPortalAccessEnabled == true)
                            {
                                // Get contacts
                                var partyWithContacts = await _partyMasterCommandRepository.GetByIdWithContactsAsync(party.Id) ?? party;
                                var contact = GetPrimaryContact(partyWithContacts)
                                            ?? await _partyMasterCommandRepository.GetPrimaryContactAsync(party.Id);

                                var LastName = contact?.LastName?.Trim();
                                var email = contact?.EmailID?.Trim();
                                var mobile = contact?.MobileNo?.Trim();

                                // NEW: company & unit ids from mapping table (Dapper)
                                var (companyIds, unitIds) = await _partyMasterQueryRepository.GetCompanyUnitMapAsync(party.Id);

                                // fallbacks (optional): if mapping empty, you can fallback to Party.UnitId
                                if (unitIds.Count == 0 && party.UnitId > 0) unitIds = new List<int> { party.UnitId };

                                var partyTypeCodes = await _partyMasterQueryRepository.GetPartyTypeCodesAsync(party.Id);

                                var evt = new PartyApprovedIntegrationEvent
                                {
                                    CorrelationId = NewId.NextGuid(),
                                    PartyId = party.Id,
                                    CompanyIds = companyIds,
                                    UnitIds = unitIds,
                                    PartyCode = party.PartyCode,
                                    PartyName = contact?.FirstName ?? party.PartyName,
                                    PartyLastName = contact?.LastName,
                                    Email = email,
                                    Mobile = mobile,
                                    DefaultRoleId = 7,                         // or your logic
                                    DefaultCompanyId = companyIds.FirstOrDefault(),
                                    DefaultUnitId = unitIds.FirstOrDefault(),
                                    PartyTypeCodes = partyTypeCodes,
                                    
                                     // --- audit ---
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = _ipAddressService.GetUserId(),
                                    CreatedByName = _ipAddressService.GetUserName(),
                                    CreatedIp = _ipAddressService.GetSystemIPAddress()

                                    // Optional: if you map role by party types, add them here
                                    // PartyTypeCodes = await _partyMasterCommandRepository.GetPartyTypeCodesAsync(party.Id)
                                };

                                await context.Publish(evt, headers =>
                                {
                                    headers.CorrelationId = evt.CorrelationId;
                                    headers.Headers.Set("user-id", _ipAddressService.GetUserId());
                                    headers.Headers.Set("user-name", _ipAddressService.GetUserName());
                                    headers.Headers.Set("ip", _ipAddressService.GetSystemIPAddress());
                                });
                            }
                        }                       

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

            private static PartyContact? GetPrimaryContact(PartyManagement.Domain.Entities.PartyMaster p) =>
                p.PartyContactTypes?
                .OrderByDescending(c => string.Equals(c.ContactBy, "Primary", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) || !string.IsNullOrWhiteSpace(c.MobileNo))
                .FirstOrDefault();                      

    }
}