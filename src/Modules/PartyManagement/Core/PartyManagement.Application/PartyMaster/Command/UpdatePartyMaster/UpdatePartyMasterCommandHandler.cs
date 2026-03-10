#nullable disable
using AutoMapper;
using Contracts.Events.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Domain.Events;
using MassTransit;
using MediatR;


namespace PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster
{
    public class UpdatePartyMasterCommandHandler : IRequestHandler<UpdatePartyMasterCommand, bool>
    {
        private readonly IPartyMasterCommandRepository _partyMasterCommandRepository;
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILocationLookup _locationLookup;  // ✅ add this

         private readonly IPublishEndpoint _publishEndpoint;
        private readonly IIPAddressService _ip;               // for audit headers

        public UpdatePartyMasterCommandHandler(IPartyMasterCommandRepository partyMasterCommandRepository, IMapper mapper, IMediator mediator, IPartyMasterQueryRepository ipartyMasterQueryRepository, ILocationLookup locationLookup,
            IPublishEndpoint bus,
            IIPAddressService ipAddressService)
        {
            _partyMasterCommandRepository = partyMasterCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _locationLookup = locationLookup;
            _publishEndpoint = bus;
            _ip = ipAddressService;
        }

        public async Task<bool> Handle(UpdatePartyMasterCommand request, CancellationToken cancellationToken)
        {
            var partyId = request.UpdatePartyMaster.Id;
             

            // --- BEFORE snapshot (to detect 0->1 portal enable, and to compare sets if you want)
            var before = await _ipartyMasterQueryRepository.GetByIdPartyMasterAsync(partyId);
            if (before == null)
                throw new ExceptionRules($"Party {partyId} not found.");

            // Fetch existing document IDs for this party from DB
            var existingDocIds = await _partyMasterCommandRepository
                .GetPartyDocumentIdsAsync(request.UpdatePartyMaster.Id);

            // Filter only new docs (not already in DB)
            var newDocuments = request.UpdatePartyMaster.PartyDocumentsUpdate?
                                .Where(d => !existingDocIds.Contains(d.DocumentId))
                                .ToList() ?? new List<UpdatePartyMasterDto.UpdatePartyDocumentDto>();
            if (newDocuments.Any())
            {
                string baseDirectory = await _ipartyMasterQueryRepository.GetBaseDirectoryAsync();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                EnsureDirectoryExists(uploadPath);

                foreach (var doc in newDocuments)
                {
                    if (string.IsNullOrWhiteSpace(doc.FileName) || doc.FileName == "string")
                        continue;

                    string oldFilePath = Path.Combine(uploadPath, doc.FileName);
                    if (File.Exists(oldFilePath))
                    {
                        // Rename file to PartyCode_DocId.ext
                        string newFileName = $"{request.UpdatePartyMaster.PartyCode}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
                        string newFilePath = Path.Combine(uploadPath, newFileName);

                        File.Move(oldFilePath, newFilePath, overwrite: true);

                        doc.FileName = newFileName;
                        doc.UploadedDate = DateTime.Now;
                    }
                }
            }

            // ------------------- Clean SalesTypes -------------------
            if (request.UpdatePartyMaster.SalesTypesUpdate != null)
            {
                request.UpdatePartyMaster.SalesTypesUpdate = request.UpdatePartyMaster.SalesTypesUpdate
                    .Where(s =>
                        s.SalesSegmentId != 0 &&
                        s.PaymentTermsId != 0)
                    .ToList();

                if (!request.UpdatePartyMaster.SalesTypesUpdate.Any())
                    request.UpdatePartyMaster.SalesTypesUpdate = null;
            }

            // Map DTO to Entity (Fix: Pass full DTO, not just Id)
            var partyEntity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(request.UpdatePartyMaster);

            // ------------------- Location Lookup -------------------
            if (request.UpdatePartyMaster.PartyAddressesUpdate != null && request.UpdatePartyMaster.PartyAddressesUpdate.Any())
            {
                for (int i = 0; i < request.UpdatePartyMaster.PartyAddressesUpdate.Count; i++)
                {
                    var addressDto = request.UpdatePartyMaster.PartyAddressesUpdate[i];
                    var addressEntity = partyEntity.PartyAddressTypes.ElementAt(i);

                    // Only call lookup if City/State/Country is provided (update case)
                    if (!string.IsNullOrWhiteSpace(addressDto.City) &&
                        !string.IsNullOrWhiteSpace(addressDto.State) &&
                        !string.IsNullOrWhiteSpace(addressDto.Country))
                    {
                        var location = await _locationLookup.GetLocationAsync(
                            addressDto.City,
                            addressDto.State,
                            addressDto.Country,
                            cancellationToken);

                        if (location == null)
                            throw new Exception("Location could not be resolved via lookup.");

                        // Assign IDs to entity
                        addressEntity.CityId = location.CityId;
                        addressEntity.StateId = location.StateId;
                        addressEntity.CountryId = location.CountryId;
                    }
                    else
                    {
                        // Keep existing IDs if City/State/Country not provided
                        addressEntity.CityId = addressEntity.CityId;
                        addressEntity.StateId = addressEntity.StateId;
                        addressEntity.CountryId = addressEntity.CountryId;
                    }
                }
            }

            // Update main entity in repository
            var result = await _partyMasterCommandRepository.UpdateAsync(partyEntity.Id, partyEntity);

            if (!result)
                throw new ExceptionRules("PartyMaster update failed.");

            // Publish Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: partyEntity.Id.ToString(),
                actionName: partyEntity.PartyName ?? "NULL",
                details: "PartyMaster details were updated",
                module: "PartyMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            
             var currentUserId   = _ip.GetUserId();
            var currentUserName = _ip.GetUserName();
            var currentIp       = _ip.GetSystemIPAddress();

            // new code for user creation

            // --- AFTER snapshot (or use request payload directly)
            // We’ll mainly use the request (already the new state), but you can re-query if you prefer.
            // AFTER/new state from request (no re-query)
            bool wasPortalEnabled = before.IsPortalAccessEnabled == 1;
            bool nowPortalEnabled = request.UpdatePartyMaster.IsPortalAccessEnabled == 1;

            // Gather “after” company/unit/partyType state
            var (companyIds, unitIds) = ExtractCompanyUnitIds(request);
            if (companyIds.Count == 0 || unitIds.Count == 0)
            {
                // fallback to DB mapping if request didn’t send them
                var (dbCompanyIds, dbUnitIds) = await _ipartyMasterQueryRepository.GetCompanyUnitMapAsync(partyId);
                if (companyIds.Count == 0) companyIds = dbCompanyIds.ToList();
                if (unitIds.Count == 0) unitIds = dbUnitIds.ToList();
            }
            // Party type codes (SUPPLIER/CUSTOMER/AGENT). Use repo to get real codes.
            var partyTypeCodes = await _ipartyMasterQueryRepository.GetPartyTypeCodesAsync(partyId);
            
            // Primary contact (optional identity)
            
            // Primary contact for email/mobile/name
            var primary = GetPrimaryContactFromRequestOrBefore(request, before);
            var email = primary?.EmailID?.Trim();
            var mobile = primary?.MobileNo?.Trim();
            var partyName = primary?.FirstName ?? request.UpdatePartyMaster.PartyName ?? before.PartyName;
            var partyLastName = primary?.LastName ;

            // === Decide which event to publish ===
            if (!wasPortalEnabled && nowPortalEnabled)
            {
                // 0 -> 1: No user? Let PartyApproved create user.
                var createEvt = new PartyApprovedIntegrationEvent
                {
                    CorrelationId = NewId.NextGuid(),
                    PartyId = partyId,
                    CompanyIds = companyIds,
                    UnitIds = unitIds,
                    PartyCode = request.UpdatePartyMaster.PartyCode ?? before.PartyCode,
                    PartyName = partyName,
                    PartyLastName = partyLastName,
                    Email = email,
                    Mobile = mobile,
                    DefaultCompanyId = companyIds.FirstOrDefault(),
                    DefaultUnitId = unitIds.FirstOrDefault(),
                    // Default role if you want to force one on create (or let consumer map party types):
                    // DefaultRoleId = 7,
                    PartyTypeCodes = partyTypeCodes,
                    IsPortalAccessEnabled = nowPortalEnabled,
                    CreatedBy = _ip.GetUserId(),
                    CreatedByName = _ip.GetUserName(),
                    CreatedIp = _ip.GetSystemIPAddress(),
                    CreatedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(createEvt, ctx =>
                {
                    ctx.CorrelationId = createEvt.CorrelationId;
                    ctx.Headers.Set("user-id", _ip.GetUserId());
                    ctx.Headers.Set("user-name", _ip.GetUserName());
                    ctx.Headers.Set("ip", _ip.GetSystemIPAddress());
                });
              
            }
            else
            {
                // Any other update (including 1->1, or 1->0, set changes, etc.): sync existing user
                var syncEvt = new PartySyncIntegrationEvent
                {
                    CorrelationId = NewId.NextGuid(),
                    PartyId = partyId,
                    IsPortalAccessEnabled = nowPortalEnabled,
                    CompanyIds = companyIds,
                    UnitIds = unitIds,
                    PartyTypeCodes = partyTypeCodes,
                    DefaultCompanyId = companyIds.FirstOrDefault(),
                    DefaultUnitId = unitIds.FirstOrDefault(),
                    PartyName = partyName,
                    PartyLastName = partyLastName,
                    Email = email,
                    Mobile = mobile,
                    ModifiedBy  = _ip.GetUserId(),
                    ModifiedByName = _ip.GetUserName(),
                    ModifiedIp = _ip.GetSystemIPAddress(),
                    ModifiedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(syncEvt, ctx =>
                {
                    ctx.CorrelationId = syncEvt.CorrelationId;
                    ctx.Headers.Set("user-id", _ip.GetUserId());
                    ctx.Headers.Set("user-name", _ip.GetUserName());
                    ctx.Headers.Set("ip", _ip.GetSystemIPAddress());
                });
            }

            return true;
            // return result;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }  
        

           // -------- helpers --------

       private static (List<int> companyIds, List<int> unitIds) ExtractCompanyUnitIds(UpdatePartyMasterCommand request)
        {
            var companyIds = new List<int>();
            var unitIds    = new List<int>();

            var puc = request.UpdatePartyMaster.PartyUnitCompaniesUpdate;
            if (puc != null && puc.Count > 0)
            {
                companyIds = puc.Where(x => x.CompanyId > 0).Select(x => x.CompanyId).Distinct().ToList();
                unitIds    = puc.Where(x => x.UnitId    > 0).Select(x => x.UnitId).Distinct().ToList();
            }

            // No top-level UnitId in UpdatePartyMasterDto – do not read it.
            return (companyIds, unitIds);
        }


        private static List<string> ExtractPartyTypeCodes(UpdatePartyMasterCommand request)
        {
            // If your PartyTypesUpdate gives only Ids, you may need repo lookup to map Id->Code.
            // If request has the code as “description” or a specific field, use that.
            // Here we try “description” first, fallback to a label.
            var codes = new List<string>();

            var types = request.UpdatePartyMaster.PartyTypesUpdate;
            if (types == null || types.Count == 0) return codes;

            // adapt: if you store the CODE elsewhere, map accordingly
            foreach (var t in types)
            {
                // e.g., the code could be “SUPPLIER/CUSTOMER/AGENT”.
                // If you only have PartyTypeId, you’d need a lookup here.
                // For now, push numeric id as string if you don’t have code.
                // Your consumer maps role names from these codes if they match.
                if (t.PartyTypeId > 0)
                    codes.Add(t.PartyTypeId.ToString()); // replace with actual code lookup if you have it
            }

            return codes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static PartyMasterDto.PartyContactDto GetPrimaryContactFromRequestOrBefore(
            UpdatePartyMasterCommand request,
            PartyMasterDto before)
        {
            // prefer request Primary contact; else fallback to before snapshot
            var pick = request.UpdatePartyMaster.PartyContactsUpdate?
                .OrderByDescending(c => string.Equals(c.ContactBy, "Primary", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) || !string.IsNullOrWhiteSpace(c.MobileNo))
                .FirstOrDefault();

            if (pick != null)
                return new PartyMasterDto.PartyContactDto
                {
                    FirstName = pick.FirstName,
                    LastName = pick.LastName,
                    EmailID = pick.EmailID,
                    MobileNo = pick.MobileNo,
                    ContactBy = pick.ContactBy
                };

            return before.PartyContacts?
                .OrderByDescending(c => string.Equals(c.ContactBy, "Primary", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) || !string.IsNullOrWhiteSpace(c.MobileNo))
                .FirstOrDefault();
        }
    }
}
